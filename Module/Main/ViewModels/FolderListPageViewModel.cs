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
using ReactiveFolder.Model.Filters;
using ReactiveFolder.Model.Timings;
using ReactiveFolder.Model.Actions;
using ReactiveFolder.Model.Destinations;

namespace Modules.Main.ViewModels
{
	public class FolderListPageViewModel : BindableBase, INavigationAware
	{
		private IRegionManager _RegionManager;
		public FolderReactionMonitorModel MonitorModel { get; private set; }

	


		public FolderListItemViewModel FolderRootListItem { get; private set; }



		

		public FolderListPageViewModel(IRegionManager regionManager, FolderReactionMonitorModel monitor)
		{
			_RegionManager = regionManager;
			MonitorModel = monitor;

			FolderRootListItem = new FolderListItemViewModel(MonitorModel.RootFolder);

//			Groups = MonitorModel.ReactionGroups
//				.ToReadOnlyReactiveCollection(x => new ReactiveFolderGroupViewModel(this, x));
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



		

		

		public void NavigationToReactionEditerPage(FolderReactionModel reaction)
		{
			var param = new NavigationParameters();
			param.Add("guid", reaction.Guid);
			this._RegionManager.RequestNavigate("MainRegion", nameof(Views.ReactionEditerPage), param);
		}
	}


	// ReactiveFolderModel.FolderModelのVM
	public class FolderListItemViewModel : BindableBase
	{
		public FolderModel Folder { get; private set; }

		public ReadOnlyReactiveCollection<ReactionListItemViewModel> ReactionListItems { get; private set; }

		public ReadOnlyReactiveCollection<FolderListItemViewModel> ChildrenFolderListItems { get; private set; }

		public FolderListItemViewModel(FolderModel folderModel)
		{
			Folder = folderModel;

			ReactionListItems = Folder.Models
				.ToReadOnlyReactiveCollection(x => new ReactionListItemViewModel(x));

			ChildrenFolderListItems = Folder.Children
				.ToReadOnlyReactiveCollection(x => new FolderListItemViewModel(x));
		}

		// TODO: Rename Folder

		// TODO: Add Folder
		// TODO: Add Reaction

		// TODO: Remove Folder
		// TODO: Remove Reaction

		/*
		private DelegateCommand _AddReactionFolderGroupCommand;
		public DelegateCommand AddReactionFolderGroupCommand
		{
			get
			{
				return _AddReactionFolderGroupCommand
					?? (_AddReactionFolderGroupCommand = new DelegateCommand(() =>
					{
						var desktop = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

						var targetDir = new System.IO.DirectoryInfo(desktop);

						var group = MonitorModel.CreateNewReactionGroup(targetDir);

						group.Name = "Test";

						var reaction = group.AddReaction();
						reaction.Name = "something reaction";

						reaction.Destination = new SameInputReactiveDestination();
						reaction.Timing = new FileUpdateReactiveTiming();
						reaction.Filter = new FileReactiveFilter();

						reaction.AddAction(new RenameReactiveAction("#{name}"));



						MonitorModel.SaveReactionGroup(group.Guid);

						NavigationToReactionEditerPage(group);
					}));
			}
		}
		*/
	}

	// ReactiveFolderModel.FolderModelに含まれるFolderReactionModelのVM
	public class ReactionListItemViewModel : BindableBase
	{
		public FolderReactionModel ReactionModel { get; private set; }

		public ReactionListItemViewModel(FolderReactionModel reactionModel)
		{
			ReactionModel = reactionModel;
		}
	}
}
