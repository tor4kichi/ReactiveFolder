using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Models.AppPolicy;
using ReactiveFolder.Models.Filters;
using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.Actions
{
	// TODO: AppPolicyやAppArgumentが削除された場合に備える
	// GetAppPolicy()を使う部分が安定しない

	[DataContract]
	public class AppLaunchReactiveAction : ReactiveActionBase
	{
		private static IAppPolicyManager _AppPolicyManager;
		public static IAppPolicyManager AppPolicyManager
		{
			get
			{
				if (_AppPolicyManager == null)
				{
					throw new Exception("AppLaunchReactiveAction want implemeted IAppPolicyFactory");
				}

				return _AppPolicyManager;
			}
		}

		public static void SetAppPolicyManager(IAppPolicyManager manager)
		{
			_AppPolicyManager = manager;
		}


		[DataMember]
		private Guid _AppGuid;
		public Guid AppGuid
		{
			get
			{
				return _AppGuid;
			}
			set
			{
				SetProperty(ref _AppGuid, value);
			}
		}


		private ApplicationPolicy _AppPolicy;
		public ApplicationPolicy AppPolicy
		{
			get
			{
				return _AppPolicy
					?? (_AppPolicy = GetAppPolicy());
			}
		}



		[DataMember]
		private ObservableCollection<AppOptionInstance> _Options { get; set; }

		public ReadOnlyObservableCollection<AppOptionInstance> Options { get; private set; }



		private ApplicationPolicy GetAppPolicy()
		{
			return AppPolicyManager.FromAppGuid(AppGuid);
		}

		public override FolderItemType InputItemType
		{
			get
			{
				return GetAppPolicy()?.InputPathType ?? FolderItemType.File;
			}
		}

		public override FolderItemType OutputItemType
		{
			get
			{
				var appPolicy = GetAppPolicy();
				if (appPolicy?.HasOutputDeclaration ?? false)
				{
					if (appPolicy.GetOutputExtentions().Count() > 0)
					{
						return FolderItemType.File;
					}
					else
					{
						return FolderItemType.Folder;
					}
				}
				else
				{
					return FolderItemType.File;
				}
			}
		}


		public AppLaunchReactiveAction()
		{
			_Options = new ObservableCollection<AppOptionInstance>();

			Options = new ReadOnlyObservableCollection<AppOptionInstance>(_Options);
		}

		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			if (_Options == null)
			{
				_Options = new ObservableCollection<AppOptionInstance>();
			}


			Options = new ReadOnlyObservableCollection<AppOptionInstance>(_Options);

			if (AppPolicy != null)
			{
				foreach (var option in Options)
				{
					option.ResetDeclaration(AppPolicy);
				}
			}
			else
			{
				// Note: appPolicyが削除されている？ 
				// Validateで失敗するように
			}
		}


		protected override ValidationResult InnerValidate()
		{
			var result = new ValidationResult();

			if (AppPolicy == null)
			{
				result.AddMessage("AppPolicy Deleted.");
				return result;
			}

			if (false == AppPolicyManager.Security.IsAuthorized(AppPolicy.ApplicationPath))
			{
				result.AddMessage("AppPolicy is not Authorized.");
				return result;
			}

			if (Options.Count == 0)
			{
				result.AddMessage("Needed one or more Options");
				return result;
			}


			if (false == CanCreateSandbox())
			{
				result.AddMessage("AppLaunchReactiveAction:Invalid AppName:guid is " + AppGuid);
				return result;
			}


			var sandbox = CreateSandbox();

			if (false == sandbox.Validate(GenerateTempStreamContext()))
			{
				result.AddMessage("may be diffarent Reaction I/O type and Action Input Type.");
			}			

			return result;
		}


		public override void Execute(ReactiveStreamContext context)
		{
			// TODO: SourcePathのファイルが存在しなかったら終了

			var sourcePath = context.SourcePath;
			var tempOutputFolder = context.TempOutputFolder;

			try
			{
				var sandbox = CreateSandbox();

				if (sandbox.Execute(sourcePath, tempOutputFolder))
				{
					context.SetNextWorkingPath(sandbox.ResultPath);
				}
				else
				{
					context.Failed("Execute failed or timeout");
				} 
			}
			catch (Exception e)
			{
				context.Failed("ReactiveStreamFailed on Update", e);
			}
		}

		public bool CanCreateSandbox()
		{
			if (AppPolicy == null)
			{
				return false;
			}

			if (false == AppPolicyManager.Security.IsAuthorized(AppPolicy.ApplicationPath))
			{
				return false;
			}

			if (false == AppPolicy.ValidateAppOptionInstances(Options))
			{
				return false;
			}

			return true;
		}


		public ApplicationExecuteSandbox CreateSandbox()
		{
			var appPolicy = AppPolicyManager.FromAppGuid(AppGuid);
			if (appPolicy == null)
			{
				throw new Exception("");
			}

			return appPolicy.CreateExecuteSandbox(AppPolicyManager, Options.ToArray());
		}

		public IEnumerable<string> GetFilters()
		{
			if (AppPolicy != null)
			{
				return AppPolicy.AcceptExtentions;
			}
			else
			{
				return Enumerable.Empty<string>();
			}
		}


		public bool HasFileOutputOption
		{
			get
			{
				return Options.Any(x => x.OptionDeclaration is AppOutputOptionDeclaration);
			}
		}

		public string GetOutputExtention()
		{
			var outputOption = Options.SingleOrDefault(x => x.OptionDeclaration is AppOutputOptionDeclaration);
			if (outputOption == null)
			{
				return null;
			}

			var outputOptDecl = outputOption.OptionDeclaration as AppOutputOptionDeclaration;
			var fileOutputOptProperty = outputOptDecl.OutputPathProperty as FileOutputAppOptionProperty;

			if (fileOutputOptProperty == null)
			{
				return null;
			}

			return fileOutputOptProperty.Extention;
		}



		public override bool Equals(ReactiveActionBase other)
		{
			if (false == other is AppLaunchReactiveAction)
			{
				return false;
			}

			var cast = other as AppLaunchReactiveAction;

			return this.AppGuid == cast.AppGuid;
		}


		/// <summary>
		/// AdditionalOptionsにオプションインスタンスを追加します。
		/// 出力オプションのインスタンスを追加する時に、AdditionalOptionsに一つしか存在しないよう既存の出力オプションが除外されます。
		/// </summary>
		/// <see cref="ApplicationPolicy.OutputOptionDeclarations"/>
		/// <param name="instance"></param>
		public void AddAppOptionInstance(AppOptionInstance instance)
		{
			
			if (AppPolicy.OutputOptionDeclarations.Contains(instance.OptionDeclaration))
			{
				var alreadyOutputOption = Options.SingleOrDefault(x => AppPolicy.OutputOptionDeclarations.Any(y => x.OptionDeclaration == y));

				if (alreadyOutputOption != null)
				{
					if (instance.OptionDeclaration == alreadyOutputOption.OptionDeclaration)
					{
						return;
					}

					_Options.Remove(alreadyOutputOption);
				}
			}

			_Options.Add(instance);

			ValidatePropertyChanged();
		}

		public void RemoveAppOptionInstance(AppOptionInstance instance)
		{
			if (_Options.Remove(instance))
			{
				ValidatePropertyChanged();
			}
		}
	}
	
}
