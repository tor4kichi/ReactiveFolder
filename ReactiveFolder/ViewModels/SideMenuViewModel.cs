using MaterialDesignThemes.Wpf;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.ViewModels
{
	public class SideMenuViewModel : BindableBase
	{
		public IRegionManager _RegionManager;

		public ReactiveFolderApp App { get; private set; }



		public MenuItemViewModel AppPolicyManageListItem { get; private set; }
		public MenuItemViewModel ReactionManageListItem { get; private set; }
		public MenuItemViewModel SettingsListItem { get; private set; }
		public MenuItemViewModel AboutListItem { get; private set; }

		public List<MenuItemViewModel> TopMenuItems { get; private set; }

		public List<MenuItemViewModel> BottomMenuItems { get; private set; }



		public SideMenuViewModel()
		{
			TopMenuItems = new List<MenuItemViewModel>();
			BottomMenuItems = new List<MenuItemViewModel>();
		}

		public SideMenuViewModel(ReactiveFolderApp app, IRegionManager regionManagar)
		{
			App = app;
			_RegionManager = regionManagar;

			
			

			AppPolicyManageListItem = new MenuItemViewModel(AppPageType.AppPolicyManage)
			{
				Title = "App Policy List",
				Kind = PackIconKind.Apps,
				MenuItemSelectedCommand = new DelegateCommand(() =>
				{
					App.PageType = AppPageType.AppPolicyManage;
				})
			};

			ReactionManageListItem = new MenuItemViewModel(AppPageType.ReactionManage)
			{
				Title = "Manage Reaction",
				Kind = PackIconKind.ViewList,
				MenuItemSelectedCommand = new DelegateCommand(() =>
				{
					App.PageType = AppPageType.ReactionManage;
				})
			};




			
			SettingsListItem = new MenuItemViewModel(AppPageType.Settings)
			{
				Title = "Settings",
				Kind = PackIconKind.Settings,
				MenuItemSelectedCommand = new DelegateCommand(() =>
				{
					App.PageType = AppPageType.Settings;
				})
			};

			AboutListItem = new MenuItemViewModel(AppPageType.About)
			{
				Title = "About",
				Kind = PackIconKind.CommentQuestionOutline,
				MenuItemSelectedCommand = new DelegateCommand(() => 
				{
					App.PageType = AppPageType.About;
				})
			};


			TopMenuItems = new List<MenuItemViewModel>()
			{
				AppPolicyManageListItem,
				ReactionManageListItem
			};

			BottomMenuItems = new List<MenuItemViewModel>()
			{
				SettingsListItem,
				AboutListItem
			};



			App.ObserveProperty(x => x.PageType)
				.Subscribe(x =>
				{
					var allMenuItems = TopMenuItems.Concat(BottomMenuItems);

					foreach(var nonSelectedItem in allMenuItems.Where(y => y.PageType != x))
					{
						nonSelectedItem.IsSelected = false;
					}

					switch (x)
					{
						case AppPageType.ReactionManage:
							ReactionManageListItem.IsSelected = true;
							break;
						case AppPageType.AppPolicyManage:
							AppPolicyManageListItem.IsSelected = true;
							break;
						case AppPageType.Settings:
							SettingsListItem.IsSelected = true;
							break;
						case AppPageType.About:
							AboutListItem.IsSelected = true;
							break;
						default:
							throw new Exception();
					}
				});
				
				

		}		
	}


	public class MenuItemViewModel : BindableBase
	{

		public MenuItemViewModel(AppPageType pageType)
		{
			PageType = pageType;

		}

		public AppPageType PageType { get; set;}
		public string Title { get; set; }
		public PackIconKind Kind { get; set; }

		public DelegateCommand MenuItemSelectedCommand { get; set; }

		private bool _IsSelected;
		public bool IsSelected
		{
			get
			{
				return _IsSelected;
			}
			set
			{
				SetProperty(ref _IsSelected, value);
			}
		}
	}
}
