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


		

		public List<MenuItemViewModel> TopMenuItems { get; private set; }

		public List<MenuItemViewModel> BottomMenuItems { get; private set; }


		public SideMenuViewModel()
		{
			TopMenuItems = new List<MenuItemViewModel>();
			BottomMenuItems = new List<MenuItemViewModel>();
		}

		public SideMenuViewModel(IRegionManager regionManagar)
		{
			_RegionManager = regionManagar;

			TopMenuItems = new List<MenuItemViewModel>();

			

			TopMenuItems.Add(new MenuItemViewModel()
			{
				Title = "App Policy List",
				Kind = PackIconKind.Apps,
				MenuItemSelectedCommand = new DelegateCommand(() =>
				{
					_RegionManager.RequestNavigate("MainRegion", nameof(Modules.AppPolicy.Views.AppPolicyListPage));
				})
			});

			TopMenuItems.Add(new MenuItemViewModel()
			{
				Title = "Manage Reaction",
				Kind = PackIconKind.ViewList,
				MenuItemSelectedCommand = new DelegateCommand(() =>
				{
					_RegionManager.RequestNavigate("MainRegion", nameof(Modules.Main.Views.FolderReactionManagePage));
				})
			});




			BottomMenuItems = new List<MenuItemViewModel>();

			BottomMenuItems.Add(new MenuItemViewModel()
			{
				Title = "Settings",
				Kind = PackIconKind.Settings,
				MenuItemSelectedCommand = new DelegateCommand(() =>
				{
					_RegionManager.RequestNavigate("MainRegion", nameof(Modules.Settings.Views.SettingsPage));
				})
			});

			BottomMenuItems.Add(new MenuItemViewModel()
			{
				Title = "About",
				Kind = PackIconKind.CommentQuestionOutline,
				MenuItemSelectedCommand = new DelegateCommand(() => 
				{
					_RegionManager.RequestNavigate("MainRegion", nameof(Modules.About.Views.AboutPage));
				})
			});
		}		
	}


	public class MenuItemViewModel
	{
		public string Title { get; set; }
		public PackIconKind Kind { get; set; }

		public DelegateCommand MenuItemSelectedCommand { get; set; }
	}
}
