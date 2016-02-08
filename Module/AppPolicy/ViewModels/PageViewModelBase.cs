﻿using Microsoft.Practices.Prism.Mvvm;
using Prism.Regions;
using ReactiveFolder.Model;
using ReactiveFolder.Model.Actions;
using ReactiveFolder.Model.AppPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.AppPolicy.ViewModels
{
	// Note: Support page to page moving.

	public class PageViewModelBase : BindableBase
	{
		protected IRegionManager _RegionManager { get; private set; }

		protected IAppPolicyManager _AppPolicyManager { get; private set; }

		public PageViewModelBase(IRegionManager regionManager, IAppPolicyManager appPolicyFactory)
		{
			_RegionManager = regionManager;
			_AppPolicyManager = appPolicyFactory;
		}



		public void NavigationToAppPolicyListPage()
		{
			_RegionManager.NavigateToAppPolicyListPage();
		}

		public void NavigationToAppPolicyEditPage(Guid appGuid)
		{
			_RegionManager.NavigateToAppPolicyEditPage(appGuid);
		}



		protected ApplicationPolicy ApplicationPolicyFromNavigationParameters(NavigationParameters param)
		{			
			var appGuid = param.GetAppPolicyName();

			var appPolicy = _AppPolicyManager.FromAppGuid(appGuid);
			if (appPolicy == null)
			{
				throw new Exception("not exists ApplicationPolicy. app name is " + appGuid);
			}

			return appPolicy;
		}


		
	}
}
