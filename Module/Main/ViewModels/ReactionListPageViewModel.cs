using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace Modules.Main.ViewModels
{
	class ReactionListPageViewModel : PageViewModelBase, INavigationAware
	{
		public FolderModel FolderModel { get; private set; }
		public IRegionNavigationService NavigationService;


		public ReactiveProperty<string> FolderName { get; private set; }

		public ReactiveProperty<bool> IsEdittingFolderName { get; private set; }
		public ReactiveProperty<bool> IsNotEdittingFolderName { get; private set; }


		public ReadOnlyReactiveCollection<ReactionListItemViewModel> ReactionListItems { get; private set; }


		public ReactionListPageViewModel(IRegionManager regionManager, FolderReactionMonitorModel monitor)
			: base(regionManager, monitor)
		{
			FolderName = new ReactiveProperty<string>("");
			IsEdittingFolderName = new ReactiveProperty<bool>(false);
			IsNotEdittingFolderName = IsEdittingFolderName.Select(x => !x)
				.ToReactiveProperty();
		}


		private void Reset()
		{
			FolderName.Value = FolderModel.Folder.Name;



			ReactionListItems = FolderModel.Models
				.ToReadOnlyReactiveCollection(x => new ReactionListItemViewModel(this, x));

			OnPropertyChanged(nameof(ReactionListItems));
		}


		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			// 入ってきたとき
			FolderModel = base.FolderModelFromNavigationParameters(navigationContext.Parameters);

			Reset();

			NavigationService = navigationContext.NavigationService;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
			// 出ていくとき
		}


	

		private DelegateCommand<object> _ExitEditFolderNameCommand;
		public DelegateCommand<object> ExitEditFolderNameCommand
		{
			get
			{
				return _ExitEditFolderNameCommand
					?? (_ExitEditFolderNameCommand = new DelegateCommand<object>(arg =>
					{
						IsEdittingFolderName.Value = false;
					}));
			}
		}



		private DelegateCommand _BackCommand;
		public DelegateCommand BackCommand
		{
			get
			{
				return _BackCommand
					?? (_BackCommand = new DelegateCommand(() =>
					{
						if (NavigationService.Journal.CanGoBack)
						{
							NavigationService.Journal.GoBack();
						}
						else
						{
							this.NavigationToFolderListPage();
						}
					}));
			}
		}

		private DelegateCommand _AddReactionCommand;
		public DelegateCommand AddReactionCommand
		{
			get
			{
				return _AddReactionCommand
					?? (_AddReactionCommand = new DelegateCommand(() =>
					{
						var desktop = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

						var targetDir = new System.IO.DirectoryInfo(desktop);

						var reaction = FolderModel.AddReaction(targetDir);
						reaction.Name = "something reaction";

						reaction.Destination = new ReactiveFolder.Model.Destinations.SameInputReactiveDestination();
						reaction.Filter = new ReactiveFolder.Model.Filters.FileReactiveFilter();

//						reaction.AddAction(new RenameReactiveAction("#{name}"));

						// save
						FolderModel.SaveReaction(reaction);

						// move to Reaction editer page.
						NavigationToReactionEditerPage(reaction);
					}));
			}
		}

		private DelegateCommand _RemoveFolderCommand;
		public DelegateCommand RemoveFolderCommand
		{
			get
			{
				return _RemoveFolderCommand
					?? (_RemoveFolderCommand = new DelegateCommand(() =>
					{
						if (FolderModel.Children.Count > 0)
						{
							// confirm delete dialog.
						}

						_MonitorModel.RootFolder.RemoveFolder(FolderModel);

						// move to Reaction editer page.
						NavigationService.Journal.GoBack();
						NavigationService.Journal.Clear();
					}));
			}
		}


	}

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
