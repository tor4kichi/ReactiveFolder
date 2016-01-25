using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;
using System.IO;

namespace Modules.Main.ViewModels
{
	public class FolderListPageViewModel : PageViewModelBase, INavigationAware
	{
		public FolderListItemViewModel FolderRootListItem { get; private set; }


		public FolderListPageViewModel(IRegionManager regionManager, FolderReactionMonitorModel monitor)
			: base(regionManager, monitor)
		{
			FolderRootListItem = new FolderListItemViewModel(this, _MonitorModel.RootFolder);
		}

		
		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
			// nothing to do.
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			// nothing to do.
		}


		private DelegateCommand _AddFolderCommand;
		public DelegateCommand AddFolderCommand
		{
			get
			{
				return _AddFolderCommand
					?? (_AddFolderCommand = new DelegateCommand(() =>
					{
						var newFolderName = "NewFolder-" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

						var folderModel = _MonitorModel.RootFolder.AddFolder(newFolderName);

						this.NavigationToFolderReactionListPage(folderModel);
					}));
			}
		}
		

		

		
	}


	

	
}
