using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using ReactiveFolder.Models;
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
		public FolderReactionManagePageViewModel PageVM { get; private set; }
		public FolderModel FolderModel { get; private set; }

		public string FolderName { get; private set; }


		public FolderListItemViewModel(FolderReactionManagePageViewModel pageVM, FolderModel folderModel)
		{
			PageVM = pageVM;
			FolderModel = folderModel;

			FolderName = folderModel.Folder.Name;

			
		}

		private DelegateCommand _SelectFolderCommand;
		public DelegateCommand SelectFolderCommand
		{
			get
			{
				return _SelectFolderCommand
					?? (_SelectFolderCommand = new DelegateCommand(() =>
					{
						PageVM.SelectFolder(FolderModel);
					}));
			}
		}

	}
}
