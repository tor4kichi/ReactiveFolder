using Microsoft.Practices.Prism.Mvvm;
using Prism.Regions;
using ReactiveFolder.Models;
using ReactiveFolderStyles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels
{
	// Note: Support page to page moving.

	abstract public class PageViewModelBase : BindableBase, INavigationAware
	{
		protected PageManager PageManager { get; private set; }

		protected IFolderReactionMonitorModel _MonitorModel { get; private set; }

		public PageViewModelBase(PageManager pageManager, IFolderReactionMonitorModel monitor)
		{
			PageManager = pageManager;
			_MonitorModel = monitor;

			
		}

		public abstract void OnNavigatedTo(NavigationContext navigationContext);
		public abstract bool IsNavigationTarget(NavigationContext navigationContext);
		public abstract void OnNavigatedFrom(NavigationContext navigationContext);
	}
}
