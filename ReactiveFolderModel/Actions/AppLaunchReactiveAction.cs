using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Models.AppPolicy;
using ReactiveFolder.Models.Filters;
using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.Actions
{
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
		private int _AppArgumentId;
		public int AppArgumentId
		{
			get
			{
				return _AppArgumentId;
			}
			set
			{
				SetProperty(ref _AppArgumentId, value);
			}
		}


		public ApplicationPolicy GetAppPolicy()
		{
			return AppPolicyFactory.FromAppGuid(AppGuid);
		}

		public override FolderItemType InputItemType
		{
			get
			{
				return GetAppPolicy().InputPathType;
			}
		}

		public override FolderItemType OutputItemType
		{
			get
			{
				return GetAppPolicy().OutputPathType;
			}
		}

		public AppLaunchReactiveAction()
		{
			
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
				
			}
		}

		public ApplicationExecuteSandbox CreateSandbox()
		{
			var appPolicy = AppPolicyFactory.FromAppGuid(AppGuid);
			if (appPolicy == null)
			{
				return null;
			}

			var appParam = appPolicy.FindArgument(this.AppArgumentId);

			if (appPolicy == null || appParam == null)
			{
				return null;
			}

			return appPolicy.CreateExecuteSandbox(appParam);
		}

		public override IEnumerable<string> GetFilters()
		{
			return GetAppPolicy().AcceptExtentions;
		}
	}
	
}
