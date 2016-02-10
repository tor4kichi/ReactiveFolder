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
using Prism.Events;
using ReactiveFolderStyles;
using System.Diagnostics;

namespace Modules.Main.ViewModels
{
	public class FolderListPageViewModel : PageViewModelBase, INavigationAware
	{
		public IRegionNavigationService NavigationService;

		private IEventAggregator _EventAggregator;

		public FolderModel CurrentFolder { get; private set; }

		public ReadOnlyReactiveCollection<ReactionListItemViewModel> ReactionItems { get; private set; }

		public ReadOnlyReactiveCollection<FolderListItemViewModel> FolderItems { get; private set; }

		public string PreviousFolderName { get; private set; }

		public ReactiveProperty<string> FolderName { get; private set; }

		public ReactiveProperty<bool> CanGoBack { get; private set; }

		public FolderListPageViewModel(IRegionManager regionManager, FolderReactionMonitorModel monitor, IEventAggregator ea)
			: base(regionManager, monitor)
		{
			_EventAggregator = ea;

			FolderName = new ReactiveProperty<string>("");
			/*
			CurrentFolder = _MonitorModel.RootFolder;

			ReactionListItems = CurrentFolder.Models
				.ToReadOnlyReactiveCollection(x => new ReactionListItemViewModel(this, x));

			ChildrenFolderListItems = CurrentFolder.Children
				.ToReadOnlyReactiveCollection(x => new FolderListItemViewModel(this, x));
				*/
			PreviousFolderName = "";

			CanGoBack = new ReactiveProperty<bool>(false);
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
			NavigationService = navigationContext.NavigationService;

			if (navigationContext.Parameters.Count() == 0)
			{
				Initialize(_MonitorModel.RootFolder);
			}
			else
			{
				var folderModel = FolderModelFromNavigationParameters(navigationContext.Parameters);
				Initialize(folderModel);
			}
		}



		private void Initialize(FolderModel folder)
		{
			CurrentFolder = folder;

			FolderName.Value = CurrentFolder.Folder.Name;

			ReactionItems = CurrentFolder.Models
				.ToReadOnlyReactiveCollection(x => new ReactionListItemViewModel(this, x));
			OnPropertyChanged(nameof(ReactionItems));

			FolderItems = CurrentFolder.Children
				.ToReadOnlyReactiveCollection(x => new FolderListItemViewModel(this, x));
			OnPropertyChanged(nameof(FolderItems));

			if (folder == _MonitorModel.RootFolder)
			{
				// ルートは戻る無効
				PreviousFolderName = "";
				OnPropertyChanged(nameof(PreviousFolderName));

				CanGoBack.Value = false;
			}
			else
			{
				var parentFolder = Path.GetDirectoryName(folder.Folder.FullName);
				PreviousFolderName = Path.GetFileName(parentFolder);
				OnPropertyChanged(nameof(PreviousFolderName));

				CanGoBack.Value = true;
			}



		}


		private DelegateCommand _BackOrOpenMenuCommand;
		public DelegateCommand BackOrOpenMenuCommand
		{
			get
			{
				return _BackOrOpenMenuCommand
					?? (_BackOrOpenMenuCommand = new DelegateCommand(() =>
					{
						if (NavigationService.Journal.CanGoBack)
						{
							NavigationService.Journal.GoBack();
						}
						else
						{
							_EventAggregator.GetEvent<PubSubEvent<ReactiveFolderPageType>>()
								.Publish(ReactiveFolderPageType.ReactiveFolder);
							// TODO: OpenSideMenu
							//							this.NavigationToFolderListPage(_MonitorModel.RootFolder);
							//							NavigationService.Journal.Clear();
						}
					}
					
					));
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
						var newFolderName = "NewFolder-" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

						var folderModel = CurrentFolder.AddFolder(newFolderName);

						this.NavigationToFolderListPage(folderModel);
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

						var reaction = CurrentFolder.AddReaction();
						reaction.Name = "something reaction";

						reaction.Filter = new ReactiveFolder.Model.Filters.FileReactiveFilter();

						//						reaction.AddAction(new RenameReactiveAction("#{name}"));

						// save
						CurrentFolder.SaveReaction(reaction);

						// move to Reaction editer page.
						NavigationToReactionEditerPage(reaction);
					}));
			}
		}

		private DelegateCommand _RemoveThisFolderCommand;
		public DelegateCommand RemoveThisFolderCommand
		{
			get
			{
				return _RemoveThisFolderCommand
					?? (_RemoveThisFolderCommand = new DelegateCommand(() =>
					{
						if (CurrentFolder.Children.Count > 0)
						{
							// confirm delete dialog.
						}

						_MonitorModel.RootFolder.RemoveFolder(CurrentFolder);

						// move to Reaction editer page.
						NavigationService.Journal.GoBack();
					}
					, () => _MonitorModel.RootFolder.Folder.FullName != this.CurrentFolder.Folder.FullName
					
					));
			}
		}
		private DelegateCommand _OpenInExplorerCommand;
		public DelegateCommand OpenInExplorerCommand
		{
			get
			{
				return _OpenInExplorerCommand
					?? (_OpenInExplorerCommand = new DelegateCommand(() =>
					{
						Process.Start(this.CurrentFolder.Folder.FullName);
					}));
			}
		}


		
	}





}
