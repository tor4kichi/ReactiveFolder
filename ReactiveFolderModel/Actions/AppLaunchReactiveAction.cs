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
		private static IAppPolicyManager _AppPolicyFactory;
		public static IAppPolicyManager AppPolicyFactory
		{
			get
			{
				if (_AppPolicyFactory == null)
				{
					throw new Exception("AppLaunchReactiveAction want implemeted IAppPolicyFactory");
				}

				return _AppPolicyFactory;
			}
		}

		public static void SetAppPolicyManager(IAppPolicyManager factory)
		{
			_AppPolicyFactory = factory;
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


		[DataMember]
		private int _AppOutputFormatId;
		public int AppOutputFormatId
		{
			get
			{
				return _AppOutputFormatId;
			}
			set
			{
				if (SetProperty(ref _AppOutputFormatId, value))
				{
					_AdditionalOptions.Clear();
				}
			}
		}


		[DataMember]
		private ObservableCollection<AppOptionInstance> _AdditionalOptions { get; set; }

		public ReadOnlyObservableCollection<AppOptionInstance> AdditionalOptions { get; private set; }



		public ApplicationPolicy GetAppPolicy()
		{
			return AppPolicyFactory.FromAppGuid(AppGuid);
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
			AdditionalOptions = new ReadOnlyObservableCollection<AppOptionInstance>(_AdditionalOptions);

			var appPolicy = GetAppPolicy();

			if (appPolicy != null)
			{
				foreach (var option in AdditionalOptions)
				{
					option.ResetDeclaration(appPolicy);
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
			return AppPolicyFactory.FromAppGuid(AppGuid) != null;
		}


		public ApplicationExecuteSandbox CreateSandbox()
		{
			var appPolicy = AppPolicyFactory.FromAppGuid(AppGuid);
			if (appPolicy == null)
			{
				throw new Exception("");
			}

			return appPolicy.CreateExecuteSandbox(AdditionalOptions.ToArray());
		}

		public IEnumerable<string> GetFilters()
		{
			var appPolicy = GetAppPolicy();
			if (appPolicy != null)
			{
				return appPolicy.AcceptExtentions;
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

			return this.AppGuid == cast.AppGuid &&
				this.AppOutputFormatId == cast.AppOutputFormatId;
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
