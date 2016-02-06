using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Model.AppPolicy;
using ReactiveFolder.Model.Filters;
using ReactiveFolder.Model.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Actions
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

		public static void SetAppPolicyFactory(IAppPolicyManager factory)
		{
			_AppPolicyFactory = factory;
		}


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
		private string _AppArgumentName;
		public string AppArgumentName
		{
			get
			{
				return _AppArgumentName;
			}
			set
			{
				SetProperty(ref _AppArgumentName, value);
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

		public override void Reaction(ReactiveStreamContext context)
		{
			var sandbox = CreateSandbox();
			var path = sandbox.Execute(context);


			// Note: contextへの書き込みはAction側の責任で行う

		}


		public ApplicationExecuteSandbox CreateSandbox()
		{
			var appPolicy = AppPolicyFactory.FromAppName(AppName);
			var appParam = appPolicy.GetOption(this.AppArgumentName);

			if (appPolicy == null || appParam == null)
			{
				return null;
			}

			return appPolicy.CreateExecuteSandbox(appParam);
		}
	}
	
}
