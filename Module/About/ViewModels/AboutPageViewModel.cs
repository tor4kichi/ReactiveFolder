using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.About.ViewModels
{
	public class AboutPageViewModel : BindableBase, INavigationAware
	{
		public IRegionManager _RegionManager;
		public IRegionNavigationService NavigationService;



		public AboutPageViewModel(IRegionManager regionManagar)
		{
			_RegionManager = regionManagar;
		}

		private DelegateCommand _BackCommand;
		public DelegateCommand BackCommand
		{
			get
			{
				return _BackCommand
					?? (_BackCommand = new DelegateCommand(() =>
					{
						if (NavigationService.Journal.CanGoBack)
						{
							NavigationService.Journal.GoBack();
						}
						else
						{
							_RegionManager.RequestNavigate("MainRegion", "FolderListPage");
						}
					}));
			}
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			NavigationService = navigationContext.NavigationService;
		}

		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
			// nothing do.
		}
	}
}
