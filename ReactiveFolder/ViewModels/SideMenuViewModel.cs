using MaterialDesignThemes.Wpf;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolderStyles.Models;
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

		public PageManager PageManager { get; private set; }



		public MenuItemViewModel AppPolicyManageListItem { get; private set; }
		public MenuItemViewModel ReactionManageListItem { get; private set; }
		public MenuItemViewModel InstantActionListItem { get; private set; }
		public MenuItemViewModel HistoryListItem { get; private set; }


		public MenuItemViewModel SettingsListItem { get; private set; }
		public MenuItemViewModel AboutListItem { get; private set; }

		public List<MenuItemViewModel> MenuItems { get; private set; }



		public SideMenuViewModel()
		{
			MenuItems = new List<MenuItemViewModel>();
		}

		public SideMenuViewModel(PageManager page, IRegionManager regionManagar)
		{
			PageManager = page;
			_RegionManager = regionManagar;

			
			

			AppPolicyManageListItem = new MenuItemViewModel(AppPageType.AppPolicyManage)
			{
				Title = "App Policy List",
				Kind = PackIconKind.ViewList,
				MenuItemSelectedCommand = new DelegateCommand(() =>
				{
					PageManager.OpenPage(AppPageType.AppPolicyManage);
				})
			};

			ReactionManageListItem = new MenuItemViewModel(AppPageType.ReactionManage)
			{
				Title = "Manage Reaction",
				Kind = PackIconKind.Monitor,
				MenuItemSelectedCommand = new DelegateCommand(() =>
				{
					PageManager.OpenPage(AppPageType.ReactionManage);
				})
			};

			InstantActionListItem = new MenuItemViewModel(AppPageType.InstantAction)
			{
				Title = "Instant Action",
				Kind = PackIconKind.Run,
				MenuItemSelectedCommand = new DelegateCommand(() =>
				{
					PageManager.OpenPage(AppPageType.InstantAction);
				})
			};

			HistoryListItem = new MenuItemViewModel(AppPageType.History)
			{
				Title = "History",
				Kind = PackIconKind.History,
				MenuItemSelectedCommand = new DelegateCommand(() =>
				{
					PageManager.OpenPage(AppPageType.History);
				})
			};



			SettingsListItem = new MenuItemViewModel(AppPageType.Settings)
			{
				Title = "Settings",
				Kind = PackIconKind.Settings,
				MenuItemSelectedCommand = new DelegateCommand(() =>
				{
					PageManager.OpenPage(AppPageType.Settings);
				})
			};

			AboutListItem = new MenuItemViewModel(AppPageType.About)
			{
				Title = "About",
				Kind = PackIconKind.CommentQuestionOutline,
				MenuItemSelectedCommand = new DelegateCommand(() => 
				{
					PageManager.OpenPage(AppPageType.About);
				})
			};


			MenuItems = new List<MenuItemViewModel>()
			{
				AppPolicyManageListItem,
				InstantActionListItem,
				ReactionManageListItem,
				HistoryListItem,

				SettingsListItem,
				AboutListItem,
			};




			PageManager.ObserveProperty(x => x.PageType)
				.Subscribe(x =>
				{
					foreach(var nonSelectedItem in MenuItems.Where(y => y.PageType != x))
					{
						nonSelectedItem.IsSelected = false;
					}

					MenuItems.Single(y => y.PageType == x).IsSelected = true;
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
