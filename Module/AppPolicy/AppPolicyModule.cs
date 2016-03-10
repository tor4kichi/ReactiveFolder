using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using ReactiveFolder.Models.AppPolicy;
using ReactiveFolderStyles.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.AppPolicy
{
	public class AppPolicyModule : IModule
	{
		IEventAggregator _EveentAggregator;
		IRegionManager _regionManager;

		public AppPolicyModule(IRegionManager regionManager, IEventAggregator ea)
		{
			_regionManager = regionManager;
			_EveentAggregator = ea;
		}

		public void Initialize()
		{
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(Views.AppPolicyManagePage));
			_regionManager.RegisterViewWithRegion("SubRegion", typeof(Views.AppPolicyEditPage));



			var openAppPolicyManagerEvent = _EveentAggregator.GetEvent<PubSubEvent<OpenAppPolicyManageEventPayload>>();
			openAppPolicyManagerEvent.Subscribe(x =>
			{
				_regionManager.RequestNavigate("MainRegion", nameof(Views.AppPolicyManagePage));
			}
			, keepSubscriberReferenceAlive: true);




			var openAppPolicyWithAppGuidEvent = _EveentAggregator.GetEvent<PubSubEvent<OpenAppPolicyWithAppGuidEventPayload>>();
			openAppPolicyWithAppGuidEvent.Subscribe(x =>
			{
				var param = AppPolicyNavigationParametersHelper.CreateNavigationParameterFromAppPolicy(x.AppPolicyGuid);
				_regionManager.RequestNavigate("MainRegion", nameof(Views.AppPolicyManagePage), param);
				_regionManager.RequestNavigate("SubRegion", nameof(Views.AppPolicyEditPage), param);
			}
			, keepSubscriberReferenceAlive: true);
		}
	}



	public static class AppPolicyRegionManagerHelper
	{
		public static void NavigateToAppPolicyListPage(this IRegionManager regionManager)
		{
			regionManager.RequestNavigate("MainRegion", nameof(AppPolicy.Views.AppPolicyManagePage));
		}

		public static void NavigateToAppPolicyEditPage(this IRegionManager regionManager, Guid appGuid)
		{
			var param = new NavigationParameters();
			param.Add("guid", appGuid);
			regionManager.RequestNavigate("MainRegion", nameof(AppPolicy.Views.AppPolicyEditPage), param);
		}
	}

	public static class AppPolicyNavigationParametersHelper
	{
		public static Guid GetAppPolicyName(this NavigationParameters param)
		{
			var appGuid = (Guid)param["guid"];
			if (appGuid == null)
			{
				throw new Exception("NavigationParameters not contains key <app>.");
			}

			return appGuid;
		}

		public static NavigationParameters CreateNavigationParameterFromAppPolicy(Guid appPolicyGuid)
		{
			var param = new NavigationParameters();
			param.Add("guid", appPolicyGuid);
			return param;
		}
	}
}
