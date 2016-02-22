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

		// TODO: NotifyPropertyChanged
		public AppInputOptionDeclaration InputOption { get; private set; }
		public AppOutputOptionDeclaration OutputOption { get; private set; }
		


		[DataMember]
		public int OptionDeclarationIdSeed { get; private set; }

		[DataMember]
		private ObservableCollection<AppOptionDeclarationBase> _OptionDeclarations { get; set; }
		public ReadOnlyObservableCollection<AppOptionDeclarationBase> OptionDeclarations { get; private set; }

		




		[DataMember]
		public int OutputFormatIdSeed { get; private set; }

		[DataMember]
		private ObservableCollection<AppOutputFormat> _AppOutputFormats { get; set; }
		public ReadOnlyObservableCollection<AppOutputFormat> AppOutputFormats { get; private set; }




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

			Guid = Guid.NewGuid();

			MaxProcessTime = TimeSpan.FromMinutes(1);

			OptionDeclarationIdSeed = 1;
			_OptionDeclarations = new ObservableCollection<AppOptionDeclarationBase>();
			OptionDeclarations = new ReadOnlyObservableCollection<AppOptionDeclarationBase>(_OptionDeclarations);

			OutputFormatIdSeed = 1;
			_AppOutputFormats = new ObservableCollection<AppOutputFormat>();
			AppOutputFormats = new ReadOnlyObservableCollection<AppOutputFormat>(_AppOutputFormats);

			_AcceptExtentions = new ObservableCollection<string>();
			AcceptExtentions = new ReadOnlyObservableCollection<string>(_AcceptExtentions);


			InputOption = new AppInputOptionDeclaration(GetNextOptionDeclarationId());
			OutputOption = new AppOutputOptionDeclaration(GetNextOptionDeclarationId());

			_OptionDeclarations.Add(InputOption);
			_OptionDeclarations.Add(OutputOption);
		}



		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			OptionDeclarations = new ReadOnlyObservableCollection<AppOptionDeclarationBase>(_OptionDeclarations);
			AppOutputFormats = new ReadOnlyObservableCollection<AppOutputFormat>(_AppOutputFormats);
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

			if (_AcceptExtentions.Contains(extention))
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






		public int GetNextOptionDeclarationId()
		{
			return OptionDeclarationIdSeed++;
		}

		public AppOptionDeclaration AddNewOptionDeclaration()
		{
			var id = GetNextOutputFormatId();
			var newOption = new AppOptionDeclaration(id);

			newOption.Name = $"Option {id}";

			_OptionDeclarations.Add(newOption);
			return newOption;
		}

		public bool RemoveOptionDeclaration(AppOptionDeclarationBase optionDecl)
		{
			return _OptionDeclarations.Remove(optionDecl);
		}


		public AppOptionDeclarationBase FindOptionDeclaration(int optionDeclId)
		{
			return _OptionDeclarations.SingleOrDefault(x => x.Id == optionDeclId);
		}




		public int GetNextOutputFormatId()
		{
			return OutputFormatIdSeed++;
		}

		public AppOutputFormat AddNewOutputFormat()
		{
			var id = GetNextOutputFormatId();
			var newArg = new AppOutputFormat(id);
			newArg.Name = $"Option {id}";


			_AppOutputFormats.Add(newArg);
			return newArg;
		}

		public bool RemoveOutputFormat(AppOutputFormat arg)
		{
			return _AppOutputFormats.Remove(arg);
		}

		
		public AppOutputFormat FindOutputFormat(int argId)
		{
			return _AppOutputFormats.SingleOrDefault(x => x.Id == argId);
		}


		public bool ExistOutputFormat(int outputFormatId)
		{
			if (outputFormatId <= 0)
			{
				return false;
			}

			return _AppOutputFormats.Any(x => x.Id == outputFormatId);
		}
	


		public string MakeCommandLineOptionText(string inputPath, DirectoryInfo outputDir, AppOutputFormat outputFormat = null, params AppOptionValueSet[] additionalOptions)
		{

			// Note: コマンドライン引数として使用するオプションを一つのリストにまとめて
			// オプションからコマンドライン引数の文字列を生成します。
			

			// input
			var inputValueSet = InputOption.CreateValueSet();
			inputValueSet.Values[0].Value = inputPath;


			// output
			var outputValueSet = OutputOption.CreateValueSet();
			string output = null;		
			switch (OutputPathType)
			{
				case FolderItemType.File:
					output = Path.Combine(outputDir.FullName, Path.GetFileName(inputPath));
					if (false == outputFormat.SameInputExtention)
					{
						output = Path.ChangeExtension(output, outputFormat.OutputExtention);
					}
					break;
				case FolderItemType.Folder:
					output = Path.Combine(outputDir.FullName, Path.GetFileNameWithoutExtension(inputPath));
					break;
				default:
					break;
			}

			outputValueSet.Values[0].Value = output;


			// input + output + outputFormat.Options + additionalOptions
			var options = 
				additionalOptions.Union(outputFormat.Options)
				.ToList();
			options.Add(inputValueSet);
			options.Add(outputValueSet);

			// finalize
			return AppOptionsToArgumentText(options);
		}

	

		private string AppOptionsToArgumentText(IEnumerable<AppOptionValueSet> optionValues)
		{
			// TODO: ValuesをDeclarations に渡してValidation CanGenerate


			var arguments = optionValues.Join(OptionDeclarations,
				(x) => x.OptionId,
				(y) => y.Id,
				(x, y) => new { Decl = y, Values = x }
				);

			var argumentTexts = arguments.OrderBy(x => x.Decl.Order)
				.Select(x =>
				{
					x.Decl.UpdateOptionValueSet(x.Values);
					return x.Decl.GenerateOptionText();
				});

			return String.Join(" ", argumentTexts);
		}
		

		public ApplicationExecuteSandbox CreateExecuteSandbox(AppOutputFormat param)
		{
			if (false == this.AppOutputFormats.Contains(param))
			{
				throw new Exception("invalid AppExecuteParam. it is diffarent ApplicationPolicy param?");
			}

			return new ApplicationExecuteSandbox(this, param);
		}


		public IEnumerable<string> GetOutputExtentions()
		{
			var outputExtentions = _AppOutputFormats.Where(x => false == x.SameInputExtention)
				.Select(x => x.OutputExtention)
				.Distinct();

			// 入力と同一の出力をする機能がある場合には、
			// 入力拡張子を出力拡張子に含める
			if (_AppOutputFormats.Any(x => x.SameInputExtention))
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


		public void RollbackFrom(ApplicationPolicy backup)
		{
			this.AppName = backup.AppName;
			this.ApplicationPath = backup.ApplicationPath;
			this.InputPathType = backup.InputPathType;
			this.OutputPathType = backup.OutputPathType;
			this.MaxProcessTime = backup.MaxProcessTime;


			// AcceptExtentions
			var remExtentions = this.AcceptExtentions.ToArray();
			foreach (var ext in remExtentions)
			{
				this.RemoveAcceptExtention(ext);
			}

			foreach (var ext in backup.AcceptExtentions)
			{
				this.AddAcceptExtention(ext);
			}

			// AppOutputFormats
			var remParams = this.AppOutputFormats.ToArray();
			foreach (var remParam in remParams)
			{
				this.RemoveOutputFormat(remParam);
			}

			foreach (var addparam in backup.AppOutputFormats)
			{
				this._AppOutputFormats.Add(addparam);
			}

			// OptionDeclaration
			var remOptionDecls = this.OptionDeclarations.ToArray();
			foreach (var remOptionDecl in remOptionDecls)
			{
				this.RemoveOptionDeclaration(remOptionDecl);
			}

			foreach (var addOptionDecl in backup.OptionDeclarations)
			{
				this._OptionDeclarations.Add(addOptionDecl);
			}
		}
	}




}
