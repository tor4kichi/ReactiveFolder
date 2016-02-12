﻿using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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


		public bool IsInactive { get; private set; }

		public bool IsInvalid { get; private set; }


		public ReactionListItemViewModel(PageViewModelBase pageVM, FolderReactionModel reactionModel)
		{
			PageVM = pageVM;
			ReactionModel = reactionModel;

			Name = ReactionModel.Name.ToString();

			var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			FilePath = ReactionModel.WorkFolder?.FullName ?? "<no setting>";

			IsInactive = false == ReactionModel.IsEnable;

			IsInvalid = false == ReactionModel.IsValid;
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
