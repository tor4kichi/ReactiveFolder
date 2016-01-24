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

			FolderRootListItem = new FolderListItemViewModel(this, MonitorModel.RootFolder);
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
		public FolderListPageViewModel PageVM { get; private set; }
		public FolderModel Folder { get; private set; }

		public string FolderName { get; private set; }

		public ReadOnlyReactiveCollection<ReactionListItemViewModel> ReactionListItems { get; private set; }

		public ReadOnlyReactiveCollection<FolderListItemViewModel> ChildrenFolderListItems { get; private set; }

		public FolderListItemViewModel(FolderListPageViewModel pageVM, FolderModel folderModel)
		{
			PageVM = pageVM;
			Folder = folderModel;


			FolderName = folderModel.Folder.Name;

			ReactionListItems = Folder.Models
				.ToReadOnlyReactiveCollection(x => new ReactionListItemViewModel(PageVM, x));

			ChildrenFolderListItems = Folder.Children
				.ToReadOnlyReactiveCollection(x => new FolderListItemViewModel(PageVM, x));
		}

		// TODO: Rename Folder

		// TODO: Add Folder
		// TODO: Add Reaction

		// TODO: Remove Folder
		// TODO: Remove Reaction

		
		private DelegateCommand _AddReactionFolderCommand;
		public DelegateCommand AddReactionFolderCommand
		{
			get
			{
				return _AddReactionFolderCommand
					?? (_AddReactionFolderCommand = new DelegateCommand(() =>
					{
						var desktop = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

						var targetDir = new System.IO.DirectoryInfo(desktop);
						
						var reaction = Folder.AddReaction(targetDir);
						reaction.Name = "something reaction";

						reaction.Destination = new SameInputReactiveDestination();
						reaction.Timing = new FileUpdateReactiveTiming();
						reaction.Filter = new FileReactiveFilter();

						reaction.AddAction(new RenameReactiveAction("#{name}"));


						// save
						Folder.SaveReaction(reaction);

						// move to Reaction editer page.
						PageVM.NavigationToReactionEditerPage(reaction);
					}));
			}
		}
		private DelegateCommand _AddFolderCommand;
		public DelegateCommand AddFolderCommand
		{
			get
			{
				return _AddFolderCommand
					?? (_AddFolderCommand = new DelegateCommand(() =>
					{
						
					}));
			}
		}
	}

	// ReactiveFolderModel.FolderModelに含まれるFolderReactionModelのVM
	public class ReactionListItemViewModel : BindableBase
	{
		public FolderListPageViewModel PageVM { get; private set; }

		public FolderReactionModel ReactionModel { get; private set; }


		public string Name { get; private set; }



		public ReactionListItemViewModel(FolderListPageViewModel pageVM, FolderReactionModel reactionModel)
		{
			PageVM = pageVM;
			ReactionModel = reactionModel;

			Name = ReactionModel.Guid.ToString();
		}
	}
}
