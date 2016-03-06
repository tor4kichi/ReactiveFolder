using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.History.ViewModels
{
	public class HisotryPageViewModel : BindableBase, INavigationAware
	{
		public HisotryPageViewModel()
		{
			
		}


		public static NavigationParameters CreateAppPolicyFilteringParameter(Guid appPolicyGuid)
		{
			var parameters = new NavigationParameters();

			parameters.Add("app_policy_guid", appPolicyGuid);

			return parameters;
		}

		public static NavigationParameters CreateReactionFilteringParameter(Guid reactionGuid)
		{
			var parameters = new NavigationParameters();

			parameters.Add("reaction_guid", reactionGuid);

			return parameters;
		}


		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
			
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			
		}
	}
}
