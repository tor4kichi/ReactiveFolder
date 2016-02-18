using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.AppPolicy
{
	
	// TODO: コマンドラインアプリのExitCodeの意味付けを設定できるようにしたい


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

			string input = null;
			string output = null;

			switch (InputPathType)
			{
				case FolderItemType.File:
					input = inputPath;
					break;
				case FolderItemType.Folder:
					input = inputPath;
					break;
				default:
					break;
			}

			switch (OutputPathType)
			{
				case FolderItemType.File:
					output = Path.Combine(outputDir.FullName, Path.GetFileName(inputPath));
					if (false == param.SameInputExtention)
					{
						output = Path.ChangeExtension(output, param.OutputExtention);
					}
					break;
				case FolderItemType.Folder:
					output = Path.Combine(outputDir.FullName, Path.GetFileNameWithoutExtension(inputPath));
					break;
				default:
					break;
			}

			argumentText = argumentText.Replace("%IN%", input);
			argumentText = argumentText.Replace("%OUT%", output);

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

		/// <summary>
		/// <para>IFolderItemOutputerの出力タイプとこのポリシーの入力タイプが一致し、かつ</para>
		/// <para>IFolderItemOutputerの出力予定のFilterの全てに対応している場合にtrueを返す</para>
		/// </summary>
		/// <param name="outputer"></param>
		/// <returns>outputerに完全対応している場合はtrue</returns>
		public bool CheckCanProcessSupport(IFolderItemOutputer outputer)
		{
			if (outputer.OutputItemType != this.InputPathType)
			{
				return false;
			}

			

			var outputTypes = outputer.GetFilters();

			return outputTypes
				.All(x => AcceptExtentions.Any(y => x == y));
		}

		/// <summary>
		/// <para>IFolderItemOutputerの出力タイプとこのポリシーの入力タイプが一致し、かつ</para>
		/// <para>IFolderItemOutputerの出力予定のFilterの 一部にでも 対応している場合にtrueを返す</para>
		/// </summary>
		/// <param name="outputer"></param>
		/// <returns></returns>
		public bool CheckCanProcessPartOfSupport(IFolderItemOutputer outputer)
		{
			if (outputer.OutputItemType != this.InputPathType)
			{
				return false;
			}

			var outputTypes = outputer.GetFilters();

			return outputTypes
				.Any(x => AcceptExtentions.Any(y => x == y));
		}
	}




}
