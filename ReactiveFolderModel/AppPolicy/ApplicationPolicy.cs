using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Model.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.AppPolicy
{
	
	// TODO: コマンドラインアプリのExitCodeの意味付けを設定できるようにしたい

	// TODO: InputとOutputのPathを含めたAppOptionをC# Scriptingで処理したい
	// -file {inputFile} -o {outputFolder} 
	// みたいな指定をしたら解釈してオプションを入れ替えてくれる感じで

	[DataContract]
	public class ApplicationPolicy : BindableBase
	{
		public static ApplicationPolicy Create(string applicationPath)
		{
			if (String.IsNullOrWhiteSpace(applicationPath))
			{
				throw new Exception(nameof(applicationPath) + " " + nameof(String.IsNullOrWhiteSpace));
			}

			var fileInfo = new FileInfo(applicationPath);

			if (false == fileInfo.Exists)
			{
				System.Diagnostics.Debug.WriteLine(applicationPath + " is invalid application path.");
				return null;
			}

			if (fileInfo.Extension != ".exe")
			{
				System.Diagnostics.Debug.WriteLine(applicationPath + " is not executable application.");
				return null;
			}

			return new ApplicationPolicy(applicationPath);
		}


		[DataMember]
		public Guid Guid { get; private set; }

		[DataMember]
		private string _AppName;
		public string AppName
		{
			get
			{

				return _AppName;
			}
			set
			{
				SetProperty(ref _AppName, value);
			}
		}

		[DataMember]
		private string _ApplicationPath;
		public string ApplicationPath
		{
			get
			{
				return _ApplicationPath;
			}
			set
			{
				if (SetProperty(ref _ApplicationPath, value))
				{
					_AppName = null;
					OnPropertyChanged(nameof(AppName));
				}
			}
		}

		[DataMember]
		private TimeSpan _MaxProcessTime;
		public TimeSpan MaxProcessTime
		{
			get
			{
				return _MaxProcessTime;
			}
			set
			{
				SetProperty(ref _MaxProcessTime, value);
			}
		}

		[DataMember]
		private string _DefaultOptionText;
		public string DefaultOptionText
		{
			get
			{
				return _DefaultOptionText;
			}
			set
			{
				SetProperty(ref _DefaultOptionText, value);
			}
		}

		[DataMember]
		private FolderItemType _InputPathType;
		public FolderItemType InputPathType
		{
			get
			{
				return _InputPathType;
			}
			set
			{
				SetProperty(ref _InputPathType, value);
			}
		}

		[DataMember]
		private FolderItemType _OutputPathType;
		public FolderItemType OutputPathType
		{
			get
			{
				return _OutputPathType;
			}
			set
			{
				SetProperty(ref _OutputPathType, value);
			}
		}

		[DataMember]
		private ObservableCollection<AppArgument> _AppParams { get; set; }
		public ReadOnlyObservableCollection<AppArgument> AppParams { get; private set; }

		[DataMember]
		public int ArgumentIdSeed { get; private set; }

		[DataMember]
		private ObservableCollection<string> _AcceptExtentions { get; set; }
		public ReadOnlyObservableCollection<string> AcceptExtentions { get; private set; }

		


		/// <summary>
		/// 
		/// </summary>
		/// <param name="applicationPath"></param>
		public ApplicationPolicy(string applicationPath)
		{
			ApplicationPath = applicationPath;
			AppName = Path.GetFileNameWithoutExtension(ApplicationPath);

			MaxProcessTime = TimeSpan.FromMinutes(1);

			DefaultOptionText = "";
			_AppParams = new ObservableCollection<AppArgument>();
			AppParams = new ReadOnlyObservableCollection<AppArgument>(_AppParams);
			_AcceptExtentions = new ObservableCollection<string>();
			AcceptExtentions = new ReadOnlyObservableCollection<string>(_AcceptExtentions);

			Guid = Guid.NewGuid();

			ArgumentIdSeed = 1;
		}



		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			AppParams = new ReadOnlyObservableCollection<AppArgument>(_AppParams);
			AcceptExtentions = new ReadOnlyObservableCollection<string>(_AcceptExtentions);
		}



		public bool IsExistApplicationFile
		{
			get
			{
				if (String.IsNullOrWhiteSpace(ApplicationPath))
				{
					return false;
				}

				return File.Exists(ApplicationPath);
			}
		}

		public bool CanAddAcceptExtention(string extention)
		{
			if (String.IsNullOrWhiteSpace(extention))
			{
				return false;
			}

			// 終端に.はng
			if(extention.EndsWith("."))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public void AddAcceptExtention(string extention)
		{
			if (false == CanAddAcceptExtention(extention))
			{
				return;
			}

			var checkedExt = extention;

			if (false == extention.StartsWith("."))
			{
				checkedExt = "." + extention;
			}

			if (_AcceptExtentions.Contains(checkedExt))
			{
				return;
			}

			_AcceptExtentions.Add(checkedExt);
		}

		public bool RemoveAcceptExtention(string extention)
		{
			return _AcceptExtentions.Remove(extention);
		}

		public int GetNextArgumentId()
		{
			return ArgumentIdSeed++;
		}

		public AppArgument AddNewArgument()
		{
			var id = GetNextArgumentId();
			var newArg = new AppArgument(id);
			newArg.Name = $"Option {id}";


			_AppParams.Add(newArg);
			return newArg;
		}

		public bool RemoveArgument(AppArgument arg)
		{
			return _AppParams.Remove(arg);
		}

		
		public AppArgument FindArgument(int argId)
		{
			return _AppParams.SingleOrDefault(x => x.Id == argId);
		}


		public string MakeArgumentsText(string inputPath, DirectoryInfo outputDir, AppArgument param)
		{
			var argumentText = $"{this.DefaultOptionText} {(param.OptionText)}";

			string outputFilePath = null;

			// 出力パス名を作成する
			// ファイルからフォルダに変換アプリの場合には、
			// 拡張子を取り外す
			if (InputPathType == FolderItemType.File && 
				OutputPathType == FolderItemType.Folder)
			{
				outputFilePath = Path.Combine(outputDir.FullName, Path.GetFileNameWithoutExtension(inputPath));
			}
			else
			{
				outputFilePath = Path.Combine(outputDir.FullName, Path.GetFileName(inputPath));
				if (false == String.IsNullOrWhiteSpace(param.OutputExtention))
				{
					outputFilePath = Path.ChangeExtension(outputFilePath, param.OutputExtention);
				}
			}

			inputPath = inputPath.Replace("\\", "/");
			outputFilePath = outputFilePath.Replace("\\", "/");

			argumentText = Regex.Replace(argumentText, "%IN_NAME%", Path.GetFileName(inputPath));
			argumentText = Regex.Replace(argumentText, "%OUT_NAME%", Path.GetFileName(outputFilePath));
			argumentText = Regex.Replace(argumentText, "%IN_PATH%", inputPath);
			argumentText = Regex.Replace(argumentText, "%OUT_PATH%", outputFilePath);
			argumentText = Regex.Replace(argumentText, "%OUT_FOLDER%", outputDir.FullName);

			return argumentText;
		}

		

		public ApplicationExecuteSandbox CreateExecuteSandbox(AppArgument param)
		{
			if (false == this.AppParams.Contains(param))
			{
				throw new Exception("invalid AppExecuteParam. it is diffarent ApplicationPolicy param?");
			}

			return new ApplicationExecuteSandbox(this, param);
		}


		public IEnumerable<string> GetOutputExtentions()
		{
			var outputExtentions = _AppParams.Where(x => false == x.SameInputExtention)
				.Select(x => x.OutputExtention)
				.Distinct();

			// 入力と同一の出力をする機能がある場合には、
			// 入力拡張子を出力拡張子に含める
			if (_AppParams.Any(x => x.SameInputExtention))
			{
				return outputExtentions.Concat(AcceptExtentions);
			}
			else
			{
				return outputExtentions;
			}
		}
	}




}
