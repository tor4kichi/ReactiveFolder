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
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(Views.AppPolicyListPage));
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(Views.AppPolicyEditPage));

//			_regionManager.RequestNavigate("MainRegion", nameof(FolderListPage));
		}
	}



	public static class AppPolicyRegionManagerHelper
	{
		public static void NavigateToAppPolicyListPage(this IRegionManager regionManager)
		{
			regionManager.RequestNavigate("MainRegion", nameof(AppPolicy.Views.AppPolicyListPage));
		}

		public static void NavigateToAppPolicyEditPage(this IRegionManager regionManager, string appName)
		{
			var param = new NavigationParameters();
			param.Add("app", appName);
			regionManager.RequestNavigate("MainRegion", nameof(AppPolicy.Views.AppPolicyEditPage), param);
		}
	}

	public static class AppPolicyNavigationParametersHelper
	{
		public static string GetAppPolicyName(this NavigationParameters param)
		{
			var appName = (string)param["app"];
			if (appName == null)
			{
				throw new Exception("NavigationParameters not contains key <app>.");
			}

			return appName;
		}
	}
}
