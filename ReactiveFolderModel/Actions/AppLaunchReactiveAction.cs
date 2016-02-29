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
		private ObservableCollection<AppOptionInstance> _AdditionalOptions { get; set; }

		public ReadOnlyObservableCollection<AppOptionInstance> AdditionalOptions { get; private set; }



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
			_AdditionalOptions = new ObservableCollection<AppOptionInstance>();

			AdditionalOptions = new ReadOnlyObservableCollection<AppOptionInstance>(_AdditionalOptions);
		}

		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			if (_AdditionalOptions == null)
			{
				_AdditionalOptions = new ObservableCollection<AppOptionInstance>();
			}


			AdditionalOptions = new ReadOnlyObservableCollection<AppOptionInstance>(_AdditionalOptions);

			if (AppPolicy != null)
			{
				foreach (var option in AdditionalOptions)
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

			if (false == CanCreateSandbox())
			{
				result.AddMessage("AppLaunchReactiveAction:Invalid AppName:guid is " + AppGuid);
				return result;
			}


			var sandbox = CreateSandbox();

			if (false == sandbox.Validate(GenerateTempStreamContext()))
			{
				result.AddMessage("Invalid AppLaunchReactiveAction, due to diffarent IO file/folder type.");
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
					context.Failed("Timeout または 実行に失敗");
				} 
			}
			catch (Exception e)
			{
				context.Failed("ReactiveStreamFailed on Update", e);
			}
		}

		public bool CanCreateSandbox()
		{
			return AppPolicy != null && AppPolicyManager.Security.IsAuthorized(AppPolicy.ApplicationPath);
		}


		public ApplicationExecuteSandbox CreateSandbox()
		{
			var appPolicy = AppPolicyManager.FromAppGuid(AppGuid);
			if (appPolicy == null)
			{
				throw new Exception("");
			}

			return appPolicy.CreateExecuteSandbox(AppPolicyManager, AdditionalOptions.ToArray());
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



		public override bool Equals(ReactiveActionBase other)
		{
			if (false == other is AppLaunchReactiveAction)
			{
				return false;
			}

			var cast = other as AppLaunchReactiveAction;

			return this.AppGuid == cast.AppGuid;
		}



		public void AddAppOptionInstance(AppOptionInstance instance)
		{
			_AdditionalOptions.Add(instance);

			ValidatePropertyChanged();
		}

		public void RemoveAppOptionInstance(AppOptionInstance instance)
		{
			if (_AdditionalOptions.Remove(instance))
			{
				ValidatePropertyChanged();
			}
		}
	}
	
}
