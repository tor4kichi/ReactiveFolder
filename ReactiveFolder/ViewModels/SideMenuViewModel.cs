using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.ViewModels
{
	public class SideMenuViewModel : BindableBase
	{
		public IRegionManager _RegionManager;

		public MainWindowViewModel OwnerWindowVM;


		public SideMenuViewModel(IRegionManager regionManagar, MainWindowViewModel mainWindow)
		{
			_RegionManager = regionManagar;
			OwnerWindowVM = mainWindow;
		}

		private DelegateCommand _OpenReactiveFolderListCommand;
		public DelegateCommand OpenReactiveFolderListCommand
		{
			get
			{
				return _OpenReactiveFolderListCommand
					?? (_OpenReactiveFolderListCommand = new DelegateCommand(() =>
					{
						_RegionManager.RequestNavigate("MainRegion", nameof(Modules.Main.Views.FolderListPage));

						OwnerWindowVM.CloseSideMenu();
					}));
			}
		}

		private DelegateCommand _OpenAppPolicyListCommand;
		public DelegateCommand OpenAppPolicyListCommand
		{
			get
			{
				return _OpenAppPolicyListCommand
					?? (_OpenAppPolicyListCommand = new DelegateCommand(() => 
					{
						_RegionManager.RequestNavigate("MainRegion", nameof(Modules.AppPolicy.Views.AppPolicyListPage));

						OwnerWindowVM.CloseSideMenu();
					}));
			}
		}
	}
}
