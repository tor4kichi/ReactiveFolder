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
		private ObservableCollection<AppOptionValueSet> _AdditionalOptions { get; set; }

		public ReadOnlyObservableCollection<AppOptionValueSet> AdditionalOptions { get; private set; }



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
				return GetAppPolicy()?.OutputPathType ?? FolderItemType.File;
			}
		}

		public AppLaunchReactiveAction()
		{
			_AdditionalOptions = new ObservableCollection<AppOptionValueSet>();

			AdditionalOptions = new ReadOnlyObservableCollection<AppOptionValueSet>(_AdditionalOptions);
		}

		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			AdditionalOptions = new ReadOnlyObservableCollection<AppOptionValueSet>(_AdditionalOptions);
		}


		protected override ValidationResult InnerValidate()
		{
			var result = new ValidationResult();

			var sandbox = CreateSandbox();

			if (sandbox == null)
			{
				result.AddMessage("AppLaunchReactiveAction:Invalid AppName or AppArgumentName");
				return result;
			}

			if (false == sandbox.Validate(GenerateTempStreamContext()))
			{
				result.AddMessage("Invalid AppLaunchReactiveAction, due to diffarent IO file/folder type.");
			}

			return result;
		}


		public override void Update(string sourcePath, DirectoryInfo destFolder)
		{
			var sandbox = CreateSandbox();

			if (sandbox == null)
			{
				throw new Exception("");
			}

			try
			{
				var path = sandbox.Execute(sourcePath, destFolder);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("外部アプリの実行に失敗しました.");
				System.Diagnostics.Debug.WriteLine(e.Message);
			}
		}

		public ApplicationExecuteSandbox CreateSandbox()
		{
			var appPolicy = AppPolicyFactory.FromAppGuid(AppGuid);
			if (appPolicy == null)
			{
				return null;
			}

			var appParam = appPolicy.FindOutputFormat(this.AppOutputFormatId);

			if (appPolicy == null || appParam == null)
			{
				return null;
			}

			return appPolicy.CreateExecuteSandbox(appParam);
		}

		public override IEnumerable<string> GetFilters()
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



		public void AddAppOptionValueSet(AppOptionValueSet valueSet)
		{
			_AdditionalOptions.Add(valueSet);

			ValidatePropertyChanged();
		}

		public void RemoveAppOptionValueSet(AppOptionValueSet valueSet)
		{
			if (_AdditionalOptions.Remove(valueSet))
			{
				ValidatePropertyChanged();
			}
		}
	}
	
}
