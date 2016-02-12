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
	// * * * * * * * * * * * * 

		
	// TODO: Initializeで渡されるworkDirが無効な場合に対応する


	// * * * * * * * * * * * * 

	[DataContract]
	public abstract class ReactiveDestinationBase : ReactiveStreamBase, IStreamContextFinalizer
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
				SetProperty(ref _OutputNamePattern, value);
			}
		}
		

		public ReactiveDestinationBase(string outputNamePattern = "")
		{
			OutputNamePattern = outputNamePattern;
		}




		abstract public string GetDistinationFolderPath();



		// for IStreamContextFinalizer
		public string OutputName { get; private set; }

		
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


		override public IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev)
		{
			return prev
				.Do(context => OutputName = FormatedString(this.OutputNamePattern, context))
				.Do(context => context.Finalize(this));
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
