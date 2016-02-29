using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.AppPolicy
{
	public class AppPolicyModule : IModule
	{
		IRegionManager _regionManager;

		public AppPolicyModule(IRegionManager regionManager)
		{
			_regionManager = regionManager;
		}

		public void Initialize()
		{
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(Views.AppPolicyManagePage));
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(Views.AppPolicyEditControl));

//			_regionManager.RequestNavigate("MainRegion", nameof(Views.AppPolicyListPage));
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
			regionManager.RequestNavigate("MainRegion", nameof(AppPolicy.Views.AppPolicyEditControl), param);
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
	}
}
