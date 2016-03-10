using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using ReactiveFolderStyles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolderStyles.ViewModels
{
	abstract public class PageViewModelBase : BindableBase, INavigationAware
	{
		public PageManager PageManager { get; private set; }

		public PageViewModelBase(PageManager pageManager)
		{
			PageManager = pageManager;
		}

		private DelegateCommand _OpenSideMenuCommand;
		public DelegateCommand OpenSideMenuCommand
		{
			get
			{
				return _OpenSideMenuCommand
					?? (_OpenSideMenuCommand = new DelegateCommand(() => 
					{
						PageManager.IsOpenSideMenu = true;
					}));
			}
		}


		private DelegateCommand _CloseSubContentCommand;
		public DelegateCommand CloseSubContentCommand
		{
			get
			{
				return _CloseSubContentCommand
					?? (_CloseSubContentCommand = new DelegateCommand(() =>
					{
						PageManager.IsOpenSubContent = false;
					}));
			}
		}



		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		abstract public void OnNavigatedTo(NavigationContext navigationContext);

		abstract public void OnNavigatedFrom(NavigationContext navigationContext);
			
	}
}
