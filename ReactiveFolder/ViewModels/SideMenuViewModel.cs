using MaterialDesignThemes.Wpf;
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


		public SideMenuViewModel(IRegionManager regionManagar)
		{
			_RegionManager = regionManagar;
		}


		public static List<MenuItemViewModel> MenuItems { get; private set; }


		static SideMenuViewModel()
		{
			MenuItems = new List<MenuItemViewModel>();

			MenuItems.Add(new MenuItemViewModel()
			{
				Title = "Manage Reaction",
				Kind = PackIconKind.Home,
				SelectedAction = (regionManager) =>
				{
					regionManager.RequestNavigate("MainRegion", nameof(Modules.Main.Views.FolderListPage));
				}
			});

			MenuItems.Add(new MenuItemViewModel()
			{
				Title = "App Policy List",
				Kind = PackIconKind.Home,
				SelectedAction = (regionManager) =>
				{
					regionManager.RequestNavigate("MainRegion", nameof(Modules.AppPolicy.Views.AppPolicyListPage));
				}
			});

			MenuItems.Add(new MenuItemViewModel()
			{
				Title = "Settings",
				Kind = PackIconKind.Settings,
				SelectedAction = (regionManager) =>
				{
					regionManager.RequestNavigate("MainRegion", nameof(Modules.Settings.Views.SettingsPage));
				}
			});

			MenuItems.Add(new MenuItemViewModel()
			{
				Title = "About",
				Kind = PackIconKind.CommentQuestionOutline,
				SelectedAction = (regionManager) =>
				{
					regionManager.RequestNavigate("MainRegion", nameof(Modules.About.Views.AboutPage));
				}
			});
		}



		private DelegateCommand<string> _OpenReactiveFolderListCommand;
		public DelegateCommand<string> MenuItemSelectedCommand
		{
			get
			{
				return _OpenReactiveFolderListCommand
					?? (_OpenReactiveFolderListCommand = new DelegateCommand<string>((menuName) =>
					{
						var menuItem = MenuItems.SingleOrDefault(x => x.Title == menuName);
						menuItem.SelectedAction(_RegionManager);
					}));
			}
		}

		
	}


	public class MenuItemViewModel
	{
		public string Title { get; set; }
		public PackIconKind Kind { get; set; }

		public Action<IRegionManager> SelectedAction { get; set; }
	}
}
