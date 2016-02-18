using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.Destinations
{
	[DataContract]
	public abstract class ReactiveDestinationBase : ReactiveStraightStreamBase
	{
		static Dictionary<string, Func<ReactiveStreamContext, string>> FormatMap;


		public const string DefaultRenamePattern = "%NAME%";
		
		static ReactiveDestinationBase()
		{
			FormatMap = new Dictionary<string, Func<ReactiveStreamContext, string>>();

			FormatMap.Add(DefaultRenamePattern, (context) => 
			{
				return Path.GetFileNameWithoutExtension(context.OriginalPath);
			});

			FormatMap.Add("%EXT%", (context) =>
			{
				return Path.GetExtension(context.OriginalPath);
			});

			FormatMap.Add("%NAME_EXT%", (context) =>
			{
				return Path.GetFileName(context.OriginalPath);
			});

			FormatMap.Add("%DATE%", (context) =>
			{
				// ToShortDateString 
				// see@: https://msdn.microsoft.com/ja-jp/library/system.datetime.toshortdatestring(v=vs.110).aspx
				return DateTime.Now.ToShortDateString().Replace("/", "");
			});

			FormatMap.Add("%DATE_%", (context) =>
			{
				return DateTime.Now.ToShortDateString().Replace("/", "_");
			});
			FormatMap.Add("%DATE-%", (context) =>
			{
				return DateTime.Now.ToShortDateString().Replace("/", "-");
			});


			FormatMap.Add("%YYYY%", (context) =>
			{
				return DateTime.Now.Year.ToString();
			});

			FormatMap.Add("%YY%", (context) =>
			{
				return (DateTime.Now.Year % 100).ToString();
			});

			FormatMap.Add("%MM%", (context) =>
			{
				return DateTime.Now.Month.ToString();
			});

			FormatMap.Add("%DD%", (context) =>
			{
				return DateTime.Now.Day.ToString();
			});

			FormatMap.Add("%HH%", (context) =>
			{
				return DateTime.Now.Hour.ToString();
			});

			FormatMap.Add("%mm%", (context) =>
			{
				return DateTime.Now.Minute.ToString();
			});

			FormatMap.Add("%SS%", (context) =>
			{
				return DateTime.Now.Second.ToString();
			});


			FormatMap.Add("%INDEX%", (context) =>
			{
				return context.Index.ToString();
			});


			FormatMap.Add("%INDEX00%", (context) => $"{context.Index:00}");
			FormatMap.Add("%INDEX000%", (context) => $"{context.Index:000}");
			FormatMap.Add("%INDEX0000%", (context) => $"{context.Index:0000}");
			FormatMap.Add("%INDEX00000%", (context) => $"{context.Index:00000}");
			FormatMap.Add("%INDEX000000%", (context) => $"{context.Index:000000}");
			FormatMap.Add("%INDEX0000000%", (context) => $"{context.Index:0000000}");
			FormatMap.Add("%INDEX00000000%", (context) => $"{context.Index:00000000}");
			FormatMap.Add("%INDEX000000000%", (context) => $"{context.Index:000000000}");

			FormatMap.Add("%INDEX--%", (context) => $"{context.Index:--}");
			FormatMap.Add("%INDEX---%", (context) => $"{context.Index:---}");
			FormatMap.Add("%INDEX----%", (context) => $"{context.Index:----}");
			FormatMap.Add("%INDEX-----%", (context) => $"{context.Index:-----}");
			FormatMap.Add("%INDEX------%", (context) => $"{context.Index:------}");
			FormatMap.Add("%INDEX-------%", (context) => $"{context.Index:-------}");
			FormatMap.Add("%INDEX--------%", (context) => $"{context.Index:--------}");
			FormatMap.Add("%INDEX---------%", (context) => $"{context.Index:---------}");

			FormatMap.Add("%INDEX__%", (context) => $"{context.Index:__}");
			FormatMap.Add("%INDEX___%", (context) => $"{context.Index:___}");
			FormatMap.Add("%INDEX____%", (context) => $"{context.Index:____}");
			FormatMap.Add("%INDEX_____%", (context) => $"{context.Index:_____}");
			FormatMap.Add("%INDEX______%", (context) => $"{context.Index:______}");
			FormatMap.Add("%INDEX_______%", (context) => $"{context.Index:_______}");
			FormatMap.Add("%INDEX________%", (context) => $"{context.Index:________}");
			FormatMap.Add("%INDEX_________%", (context) => $"{context.Index:_________}");

		}

		

		[DataMember]
		private string _OutputNamePattern;
		public string OutputNamePattern
		{
			get
			{
				return _OutputNamePattern;
			}
			set
			{
				if (SetProperty(ref _OutputNamePattern, value))
				{
					ValidatePropertyChanged();
				}
			}
		}
		

		public ReactiveDestinationBase(string outputNamePattern = "")
		{
			OutputNamePattern = outputNamePattern;
		}




		abstract public string GetDistinationFolderPath();



		
		// for IStreamContextFinalizer
		public DirectoryInfo GetDestinationFolder()
		{
			var path = GetDistinationFolderPath();

			// check
			if (path == null)
			{
				return null;
			}

			if (String.IsNullOrEmpty(path))
			{
				return null;
			}

			if (false == Path.IsPathRooted(path))
			{
				return null;
			}

			return new DirectoryInfo(path);
		}


		public override void Execute(ReactiveStreamContext context)
		{
			// TODO: OutputNameを状態として保持するのは変やで、一時変数に変更しよう
			var outputPath = FormatedString(this.OutputNamePattern, context);

			try
			{


				// やること
				// アウトプットしたいファイルまたはフォルダをNameに変更した上でDestinationFolderに移動させる

				// 例外状況
				// DestinationFolderとWorkFolderが同じ場合、かつ
				// 拡張子を含むOriginalのファイル名とOutputPathのファイル名が同じ場合、かつ
				// IsProtecteOriginalがtrueのときに
				// Destinationへの配置が実行できない状況が生まれる



				// 検証が必要：フォルダでも同様に配置不可な状況が検知できるか

				var destFolder = GetDestinationFolder();
				if (context.WorkFolder.FullName == destFolder.FullName &&
					Path.GetFileName(context.OriginalPath) == Path.GetFileName(context.SourcePath) &&
					context.IsProtectOriginal == true
					)
				{
					// Finalizeに失敗？
					outputPath = null;
					throw new Exception("OutputItem cant deployment to destination folder.");
				}

				// OutputPathで指定されたファイルをfinalizer.DestinationFolderにコピーする
				// コピーする前にファイル名をNameに変更する（拡張子はOutputPathのものを引き継ぐ）

				string outputName = Path.GetFileNameWithoutExtension(context.OriginalPath);
				if (false == String.IsNullOrWhiteSpace(outputName))
				{
					outputName = outputPath;
				}

				if (context.IsFile)
				{
					outputPath = FinalizeFile(context, outputName, destFolder);
				}
				else
				{
					outputPath = FinalizeFolder(context, outputName, destFolder);
				}

				if (outputPath != null)
				{
					context.Complete(outputPath);
				}
			}
			catch (Exception e)
			{
				context.Failed("ReactiveStreamFailed on Finalize", e);
			}
		}


		private string FinalizeFile(ReactiveStreamContext context, string outputName, DirectoryInfo destFolder)
		{

			var outputFileInfo = new FileInfo(context.SourcePath);
			if (false == outputFileInfo.Exists)
			{
				return null;
			}

			var extention = Path.GetExtension(context.SourcePath);

			var finalizeFilePath = Path.Combine(
				destFolder.FullName,
				Path.ChangeExtension(
					outputName,
					extention
				)
			);

			try
			{
				var destFileInfo = new FileInfo(finalizeFilePath);
				if (destFileInfo.Exists)
				{
					destFileInfo.Delete();
				}
				outputFileInfo.CopyTo(destFileInfo.FullName);
				return finalizeFilePath;
			}
			catch (Exception e)
			{
				context.Failed("ReactiveStreamFailed on Finalize file", e);
			}

			return null;
		}

		private string FinalizeFolder(ReactiveStreamContext context, string outputName, DirectoryInfo destFolder)
		{
			var outputFolderInfo = new DirectoryInfo(context.SourcePath);
			if (false == outputFolderInfo.Exists)
			{
				return null;
			}


			var finalizeFolderPath = Path.Combine(
				destFolder.FullName,
				outputName
			);

			var finalizeFolderInfo = new DirectoryInfo(finalizeFolderPath);
			try
			{
				if (finalizeFolderInfo.Exists)
				{
					finalizeFolderInfo.Delete();
				}

				outputFolderInfo.MoveTo(finalizeFolderPath);
				return finalizeFolderPath;
			}
			catch (Exception e)
			{
				context.Failed("ReactiveStreamFailed on Finalize folder", e);
			}

			return null;
		}


		override public void Initialize(DirectoryInfo workDir)
		{
			var destFolder = GetDestinationFolder();

			if(destFolder == null)
			{
				return;
			}

			if (false == destFolder.Exists)
			{
				// Note: 勝手につくちゃっていいの？
				destFolder.Create();
			}

			base.Initialize(workDir);
		}


		protected override ValidationResult InnerValidate()
		{
			var result = new ValidationResult();

			var destFolder = GetDestinationFolder();
			if (destFolder == null)
			{
				result.AddMessage("出力先のフォルダの指定に問題があります。");
			}

			if (destFolder != null && false == destFolder.Exists)
			{
				result.AddMessage("出力先のフォルダが存在しません。");
			}


			try
			{
				var renamed = TestRename();

				if (String.IsNullOrWhiteSpace(renamed))
				{
					result.AddMessage("failed Rename process. empty OutputNamePattern ?");
				}
				else
				{
					var invalidChars = Path.GetInvalidFileNameChars();
					if (renamed.Intersect(invalidChars).DefaultIfEmpty() == Enumerable.Empty<char>())
					{
						result.AddMessage("変換後のRenameテキストにファイル名に使用できない文字列が含まれています。");
					}
				}

			}
			catch(Exception e)
			{
				result.AddMessage("出力名の評価に失敗しました。");
				result.AddMessage(e.Message);
			}
			

			return result;
		}


		public string TestRename()
		{
			var dummyContext = GenerateTempStreamContext();

			var text = FormatedString(OutputNamePattern, dummyContext);

			return text;
		}

		public string TestRename(string renamePattern)
		{
			var dummyContext = GenerateTempStreamContext();
			
			var text = FormatedString(renamePattern, dummyContext);

			return text;
		}

		
		

		public static string FormatedString(string renamePattern, ReactiveStreamContext context)
		{
			if (String.IsNullOrWhiteSpace(renamePattern))
			{
				return FormatMap[DefaultRenamePattern](context);
			}

			var text = renamePattern;
			foreach(var key in FormatMap.Keys)
			{
				if (false == text.Contains("%"))
				{
					break;
				}

				if (text.Contains(key))
				{
					text = text.Replace(key, FormatMap[key](context));
				}
			}

			return text;
		}
	}
}
