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
				// ファイルパスの正規化
				if (false == String.IsNullOrEmpty(value))
				{
					var info = new FileInfo(value);

					if (false == info.Exists)
					{
						throw new FileNotFoundException("file path can not assign to ApplicationPath", value);
					}

					value = info.FullName;
				}

				if (SetProperty(ref _ApplicationPath, value))
				{
					if (string.IsNullOrWhiteSpace(AppName) && false == string.IsNullOrEmpty(_ApplicationPath))
					{
						AppName = Path.GetFileNameWithoutExtension(_ApplicationPath);
					}
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
				if (SetProperty(ref _InputPathType, value))
				{
					if (SameInputOutputOption != null)
					{
						SameInputOutputOption.OutputType = _InputPathType;
					}
				}
			}
		}


		
		// TODO: NotifyPropertyChanged
		public AppInputOptionDeclaration InputOption { get; private set; }
		
		public AppOutputOptionDeclaration SameInputOutputOption { get; private set; }

		[DataMember]
		public int OptionDeclarationIdSeed { get; private set; }

		[DataMember]
		private ObservableCollection<AppOptionDeclaration> _OptionDeclarations { get; set; }
		public ReadOnlyObservableCollection<AppOptionDeclaration> OptionDeclarations { get; private set; }

		[DataMember]
		private ObservableCollection<AppOutputOptionDeclaration> _OutputOptionDeclarations { get; set; }
		public ReadOnlyObservableCollection<AppOutputOptionDeclaration> OutputOptionDeclarations { get; private set; }



		[DataMember]
		private ObservableCollection<string> _AcceptExtentions { get; set; }
		public ReadOnlyObservableCollection<string> AcceptExtentions { get; private set; }





		/// <summary>
		/// 
		/// </summary>
		/// <param name="applicationPath"></param>
		public ApplicationPolicy()
		{
			ApplicationPath = "";
			AppName = "";

			Guid = Guid.NewGuid();

			MaxProcessTime = TimeSpan.FromMinutes(1);

			OptionDeclarationIdSeed = 1;
			_OptionDeclarations = new ObservableCollection<AppOptionDeclaration>();
			OptionDeclarations = new ReadOnlyObservableCollection<AppOptionDeclaration>(_OptionDeclarations);

			_OutputOptionDeclarations = new ObservableCollection<AppOutputOptionDeclaration>();
			OutputOptionDeclarations = new ReadOnlyObservableCollection<AppOutputOptionDeclaration>(_OutputOptionDeclarations);

			_AcceptExtentions = new ObservableCollection<string>();
			AcceptExtentions = new ReadOnlyObservableCollection<string>(_AcceptExtentions);


			InputOption = new AppInputOptionDeclaration("IN", GetNextOptionDeclarationId());
			SameInputOutputOption = new AppOutputOptionDeclaration("SameInput OutputExtention", GetNextOptionDeclarationId(), FolderItemType.File);
		}



		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			OptionDeclarations = new ReadOnlyObservableCollection<AppOptionDeclaration>(_OptionDeclarations);
			OutputOptionDeclarations = new ReadOnlyObservableCollection<AppOutputOptionDeclaration>(_OutputOptionDeclarations);
			AcceptExtentions = new ReadOnlyObservableCollection<string>(_AcceptExtentions);

			InputOption = new AppInputOptionDeclaration("IN", GetNextOptionDeclarationId());
			SameInputOutputOption = new AppOutputOptionDeclaration("SameInput OutputExtention", GetNextOptionDeclarationId(), FolderItemType.File);
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

		public AppOptionDeclaration AddOptionDeclaration(string name)
		{
			var id = GetNextOptionDeclarationId();
			var newOption = new AppOptionDeclaration($"OPT", id);

			newOption.Name = name;

			_OptionDeclarations.Add(newOption);
			return newOption;
		}

		public AppOutputOptionDeclaration AddOutputOptionDeclaration(string name)
		{
			var id = GetNextOptionDeclarationId();
			var newOption = new AppOutputOptionDeclaration("Extention", id, FolderItemType.File);

			newOption.Name = name;

			_OutputOptionDeclarations.Add(newOption);
			return newOption;
		}

		public bool RemoveOptionDeclaration(AppOptionDeclaration optionDecl)
		{
			return _OptionDeclarations.Remove(optionDecl);
		}

		public bool RemoveOutputOptionDeclaration(AppOutputOptionDeclaration optionDecl)
		{
			return _OutputOptionDeclarations.Remove(optionDecl);
		}


		public AppOptionDeclarationBase FindOptionDeclaration(int optionDeclId)
		{
			return GetAllDeclarations().SingleOrDefault(x => x.Id == optionDeclId);
		}


		public IEnumerable<AppOptionDeclarationBase> GetAllDeclarations()
		{
			yield return InputOption;
			yield return SameInputOutputOption;

			foreach (var opt in OptionDeclarations)
			{
				yield return opt;
			}

			foreach (var opt in OutputOptionDeclarations)
			{
				yield return opt;
			}
		}


		public bool HasOutputDeclaration
		{
			get
			{
				return OutputOptionDeclarations.Count > 0;
			}
		}

		/// <summary>
		/// 出力する拡張子がDeclarationで明示されている場合はDeclarationに従って出力拡張子を返し
		/// 出力Declarationがない場合には入力受付拡張子を返す
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetOutputExtentions()
		{
			var outputOptions = OutputOptionDeclarations
				.Where(x => x.OutputType == FolderItemType.File);

			if (outputOptions.Count() != 0)
			{
				return outputOptions
					.Where(x => x.OutputPathProperty is FileOutputAppOptionProperty)
					.Select(x => x.OutputPathProperty as FileOutputAppOptionProperty)
					.Select(x => x.Extention)
					.Distinct();
			}
			else
			{
				return AcceptExtentions;
			}
		}


		/// <summary>
		/// 渡された拡張子の"全て"が、このアプリポリシーで受け取り可能であれば true を返します。
		/// </summary>
		/// <param name="outerExts"></param>
		/// <returns></returns>
		public bool CheckCanAcceptExntentions(IEnumerable<string> outerExts)
		{
			var innerExts = GetOutputExtentions();

			// innerExtsがouterExtsの全てを含んでいればtrue

			return outerExts.All(outerExt => innerExts.Any(innerExt => outerExt.EndsWith(innerExt)));
		}



		public string AppOptionsToArgumentText(IEnumerable<AppOptionInstance> optionValues)
		{
			// TODO: ValuesをDeclarations に渡してValidation CanGenerate

			var decls = GetAllDeclarations().ToArray();

			var arguments = optionValues.Join(decls,
				(x) => x.OptionId,
				(y) => y.Id,
				(x, y) => new { Decl = y, ValueSet = x }
				);

			var argumentTexts = arguments.OrderBy(x => x.Decl.Order)
				.Select(x =>
				{
					x.Decl.UpdateOptionValueSet(x.ValueSet);
					return x.Decl.GenerateOptionText();
				});

			return String.Join(" ", argumentTexts);
		}


		public bool ValidateAppOptionInstances(IEnumerable<AppOptionInstance> options)
		{
			foreach (var opt in options)
			{
				if ( false == opt.Validate())
				{
					return false;
				}
			}

			return true;
		}

		

		public ApplicationExecuteSandbox CreateExecuteSandbox(IAppPolicyManager manager, AppOptionInstance[] options)
		{
			if (manager.Security.IsAuthorized(this.ApplicationPath))
			{
				return new ApplicationExecuteSandbox(this, options);
			}
			else
			{
				return null;
			}
		}
		

		
		

		public void RollbackFrom(ApplicationPolicy backup)
		{
			this.AppName = backup.AppName;
			this.ApplicationPath = backup.ApplicationPath;
			this.InputPathType = backup.InputPathType;
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

			// OptionDeclaration
			var remOutputOptionDecls = this.OutputOptionDeclarations.ToArray();
			foreach (var remOutputOptionDecl in remOutputOptionDecls)
			{
				this.RemoveOutputOptionDeclaration(remOutputOptionDecl);
			}

			foreach (var addOutputOptionDecl in backup.OutputOptionDeclarations)
			{
				this._OutputOptionDeclarations.Add(addOutputOptionDecl);
			}
		}
	}




}
