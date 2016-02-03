using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels
{
	// ReactiveFolderModel.FolderModelのVM
	public class FolderListItemViewModel : BindableBase
	{
		public PageViewModelBase PageVM { get; private set; }
		public FolderModel FolderModel { get; private set; }

		public string FolderName { get; private set; }


		public FolderListItemViewModel(PageViewModelBase pageVM, FolderModel folderModel)
		{
			PageVM = pageVM;
			FolderModel = folderModel;

			FolderName = folderModel.Folder.Name;

			
		}

		// TODO: Rename Folder

		// TODO: Add Folder
		// TODO: Add Reaction

		// TODO: Remove Folder
		// TODO: Remove Reaction






		private DelegateCommand _OpenFolderReactionListCommand;
		public DelegateCommand OpenFolderReactionListCommand
		{
			get
			{
				return _OpenFolderReactionListCommand
					?? (_OpenFolderReactionListCommand = new DelegateCommand(() =>
					{
						PageVM.NavigationToFolderListPage(FolderModel);
					}));
			}
		}

	}
}
