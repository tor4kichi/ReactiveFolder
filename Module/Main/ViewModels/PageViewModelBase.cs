using Microsoft.Practices.Prism.Mvvm;
using Prism.Regions;
using ReactiveFolder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels
{
	// Note: Support page to page moving.

	public class PageViewModelBase : BindableBase
	{
		protected IRegionManager _RegionManager { get; private set; }

		protected IFolderReactionMonitorModel _MonitorModel { get; private set; }

		public PageViewModelBase(IRegionManager regionManager, IFolderReactionMonitorModel monitor)
		{
			_RegionManager = regionManager;
			_MonitorModel = monitor;

			
		}



		public void NavigationToFolderListPage(FolderModel folderModel)
		{
			var param = new NavigationParameters();
			param.Add("path", folderModel.Folder.FullName);
			this._RegionManager.RequestNavigate("MainRegion", nameof(Views.FolderReactionManagePage), param);
		}



		protected FolderModel FolderModelFromNavigationParameters(NavigationParameters param)
		{
			var folderPath = (string)param["path"];
			if (folderPath == null)
			{
				throw new Exception("NavigationParameters not contains key <path>.");
			}

			// FolderModelを検索
			var folderModel = _MonitorModel.FindFolder(folderPath);
			if (folderModel == null)
			{
				throw new Exception("not exists FolderModel. path is " + folderPath);
			}

			return folderModel;
		}
				
	}
}
