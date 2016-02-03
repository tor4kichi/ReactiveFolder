using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;
using System.IO;
using ReactiveFolder.Model.AppPolicy;

namespace Modules.AppPolicy.ViewModels
{
	public class AppPolicyListPageViewModel : PageViewModelBase, INavigationAware
	{

		public IRegionNavigationService NavigationService;

		public IAppPolicyManager AppPolicyManager { get; private set; }


		public AppPolicyListPageViewModel(IRegionManager regionManager, IAppPolicyManager appPolicyManager)
			: base(regionManager, appPolicyManager)
		{
			
		}


		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
			// nothing to do.
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			NavigationService = navigationContext.NavigationService;

			
		}
	}
}
