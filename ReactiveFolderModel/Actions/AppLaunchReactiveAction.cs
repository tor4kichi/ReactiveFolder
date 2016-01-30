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

	public interface IAppPolicyFactory
	{
		IEnumerable<ApplicationPolicy> GetPolicies();

		IEnumerable<ApplicationPolicy> GetPoliciesWithFilter(FileReactiveFilter filter);

		ApplicationPolicy FromAppName(string name);

		void AddAppPolicy(ApplicationPolicy policy);
		void RemoveAppPolicy(ApplicationPolicy policy);

	}


	[DataContract]
	public class AppLaunchReactiveAction : ReactiveActionBase
	{
		private static IAppPolicyFactory _AppPolicyFactory;
		public static IAppPolicyFactory AppPolicyFactory
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

		public static void SetAppPolicyFactory(IAppPolicyFactory factory)
		{
			_AppPolicyFactory = factory;
		}


		[DataMember]
		private string _ApplicationName;
		public string ApplicationName
		{
			get
			{
				return _ApplicationName;
			}
			set
			{
				if (SetProperty(ref _ApplicationName, value))
				{
					_Sandbox = null;
				}
			}
		}


		[DataMember]
		private string _ParamterSetName;
		public string ParamterSetName
		{
			get
			{
				return _ParamterSetName;
			}
			set
			{
				if (SetProperty(ref _ParamterSetName, value))
				{
					_Sandbox = null;
				}
			}
		}


		private ApplicationExecuteSandbox _Sandbox;
		public ApplicationExecuteSandbox Sandbox
		{
			get
			{
				if (_Sandbox == null)
				{
					var appPolicy = AppPolicyFactory.FromAppName(ApplicationName);
					var appParam = appPolicy.GetOption(this.ParamterSetName);

					_Sandbox = appPolicy.CreateExecuteSandbox(appParam);
				}

				return _Sandbox;
			}
		}


		public AppLaunchReactiveAction()
		{
			
		}


		protected override ValidationResult InnerValidate()
		{
			var result = new ValidationResult();

			if (false == Sandbox.Validate(GenerateTempStreamContext()))
			{
				result.AddMessage("Invalid AppLaunchReactiveAction, due to diffarent IO file/folder type.");
			}

			return result;
		}

		public override void Reaction(ReactiveStreamContext context)
		{
			var path = Sandbox.Execute(context);


			// Note: contextへの書き込みはAction側の責任で行う

		}
	}
	
}
