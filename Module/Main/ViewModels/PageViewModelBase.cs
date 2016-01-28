﻿using Microsoft.Practices.Prism.Mvvm;
using Prism.Regions;
using ReactiveFolder.Model;
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

		protected FolderReactionMonitorModel _MonitorModel { get; private set; }

		public PageViewModelBase(IRegionManager regionManager, FolderReactionMonitorModel monitor)
		{
			_RegionManager = regionManager;
			_MonitorModel = monitor;

			
		}



		public void NavigationToFolderListPage()
		{
			this._RegionManager.RequestNavigate("MainRegion", nameof(Views.FolderListPage));
		}






		public void NavigationToFolderReactionListPage(FolderModel folderModel)
		{
			var param = new NavigationParameters();
			param.Add("path", folderModel.Folder.FullName);
			this._RegionManager.RequestNavigate("MainRegion", nameof(Views.ReactionListPage), param);
		}

		protected FolderModel FolderModelFromNavigationParameters(NavigationParameters param)
		{
			var folderPath = (string)param["path"];
			if (folderPath == null)
			{
				throw new Exception("NavigationParameters not contains key <path>.");
			}

			// FolderModelを検索
			var folderModel = _MonitorModel.RootFolder.Children.SingleOrDefault(x => x.Folder.FullName == folderPath);
			if (folderModel == null)
			{
				throw new Exception("not exists FolderModel. path is " + folderPath);
			}

			return folderModel;
		}






		public void NavigationToReactionEditerPage(FolderReactionModel reaction)
		{
			var param = new NavigationParameters();
			param.Add("guid", reaction.Guid);
			this._RegionManager.RequestNavigate("MainRegion", nameof(Views.ReactionEditerPage), param);
		}

		protected FolderReactionModel ReactionModelFromNavigationParameters(NavigationParameters param)
		{
			var guid = (Guid)param["guid"];
			if (guid == null)
			{
				throw new Exception("NavigationParameters not contains key <path>.");
			}

			var reaction = _MonitorModel.FindReaction(guid);
			if (reaction == null)
			{
				throw new Exception("not exists FolderReactionModel. guid is " + guid.ToString());
			}

			return reaction;
		}

		
		
	}
}