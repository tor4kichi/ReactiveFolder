﻿using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels
{
	// ReactiveFolderModel.FolderModelに含まれるFolderReactionModelのVM
	public class ReactionListItemViewModel : BindableBase
	{
		public PageViewModelBase PageVM { get; private set; }

		public FolderReactionModel ReactionModel { get; private set; }


		public string Name { get; private set; }

		public string FilePath { get; private set; }


		public ReactionListItemViewModel(PageViewModelBase pageVM, FolderReactionModel reactionModel)
		{
			PageVM = pageVM;
			ReactionModel = reactionModel;

			Name = ReactionModel.Name.ToString();

			var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			var isUnderUserFolder = ReactionModel.WorkFolder.FullName.IndexOf(userFolder) == 0;
			if (isUnderUserFolder)
			{
				FilePath = "<user>" + ReactionModel.WorkFolder.FullName.Substring(userFolder.Length);
			}
			else
			{
				FilePath = ReactionModel.WorkFolder.FullName;
			}

		}



		private DelegateCommand _OpenReactionCommand;
		public DelegateCommand OpenReactionCommand
		{
			get
			{
				return _OpenReactionCommand
					?? (_OpenReactionCommand = new DelegateCommand(() =>
					{
						PageVM.NavigationToReactionEditerPage(ReactionModel);
					}));
			}
		}

	}
}