using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using ReactiveFolder.Models;
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
using Microsoft.Win32;
using ReactiveFolder.Models.Util;
using ReactiveFolder.Models.AppPolicy;
using MaterialDesignThemes.Wpf;
using System.Reactive.Linq;

namespace Modules.Main.ViewModels
{
	public class FolderReactionManagePageViewModel : PageViewModelBase, INavigationAware
	{
		public IRegionNavigationService NavigationService;

		private IEventAggregator _EventAggregator;

		public IAppPolicyManager AppPolicyManager { get; private set; }

		public FolderModel CurrentFolder { get; private set; }

		public ReadOnlyReactiveCollection<ReactionListItemViewModel> ReactionItems { get; private set; }

		public ReadOnlyReactiveCollection<FolderListItemViewModel> FolderItems { get; private set; }

		public string PreviousFolderName { get; private set; }

		public ReactiveProperty<string> FolderName { get; private set; }

		public ReactiveProperty<bool> CanGoBack { get; private set; }


		public ReactiveProperty<ReactionEditControlViewModel> ReactionEditControl { get; private set; }

		public FolderReactionManagePageViewModel(IRegionManager regionManager, IFolderReactionMonitorModel monitor, IEventAggregator ea, IAppPolicyManager appPolicyManager)
			: base(regionManager, monitor)
		{
			_EventAggregator = ea;
			AppPolicyManager = appPolicyManager;

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

			ReactionEditControl = new ReactiveProperty<ReactionEditControlViewModel>();

			Initialize(_MonitorModel.RootFolder);
		}


		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
			ResumeMonitor();
		}

		public async Task<bool?> ShowReactionEditCloseConfirmDialog()
		{
			var view = new Views.DialogContent.ReactionSaveAndBackConfirmDialogContent();

			return (bool?)await DialogHost.Show(view, "ReactionEditCommonDialogHost");
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			NavigationService = navigationContext.NavigationService;
			/*
			if (navigationContext.Parameters.Count() == 0)
			{
				Initialize(_MonitorModel.RootFolder);
			}
			else
			{
				var folderModel = FolderModelFromNavigationParameters(navigationContext.Parameters);
				Initialize(folderModel);
			}
			*/
		}



		private void Initialize(FolderModel folder)
		{
			CurrentFolder = folder;

			FolderName.Value = CurrentFolder.Folder.Name;

			ReactionItems = CurrentFolder.Reactions
				.ToReadOnlyReactiveCollection(x => new ReactionListItemViewModel(this, x));

			FolderItems = CurrentFolder.Children
				.ToReadOnlyReactiveCollection(x => new FolderListItemViewModel(this, x));

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


		private DelegateCommand _RefreshCommand;
		public DelegateCommand RefreshCommand
		{
			get
			{
				return _RefreshCommand
					?? (_RefreshCommand = new DelegateCommand(() =>
					{
						CurrentFolder.UpdateReactionModels();
						CurrentFolder.UpdateChildren();
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
						var reaction = new FolderReactionModel();

						reaction.Name = "TypeYourReactionNameHere";

						reaction.Filter = new ReactiveFolder.Models.Filters.FileReactiveFilter();

						// AddReaction中で非同期での保存処理が走る
						Task.Run(() => 
						{
							CurrentFolder.AddReaction(reaction);
						})
						.ContinueWith(x => 
						{
							ShowReaction(reaction);
						});
					}));
			}
		}



		private DelegateCommand _ImportReactionCommand;
		public DelegateCommand ImportReactionCommand
		{
			get
			{
				return _ImportReactionCommand
					?? (_ImportReactionCommand = new DelegateCommand(() =>
					{

						var dialog = new OpenFileDialog();

						dialog.Title = "ReactiveFolder - Import Reaction File";
						dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

						dialog.Filter = $"Reaction File|*{FolderModel.REACTION_EXTENTION}|Json|*.json|All|*.*";
						dialog.Multiselect = true;

						var result = dialog.ShowDialog();


						if (result != null && ((bool)result) == true)
						{
							foreach (var destFilePath in dialog.FileNames)
							{
								ImportReactionFile(destFilePath);
							}
						}

						// move to Reaction editer page.
						//						NavigationToReactionEditerPage(reaction);
					}));
			}
		}

		private void ImportReactionFile(string path)
		{
			var importedReaction = FileSerializeHelper.LoadAsync<FolderReactionModel>(path);

			if (null != _MonitorModel.FindReaction(importedReaction.Guid))
			{
				// alread exist reaction
				// Guidを張り替える？
			}
			else
			{
				CurrentFolder.AddReaction(importedReaction);
			}
		}




		internal void ShowReaction(FolderReactionModel reaction)
		{
			if (reaction != ReactionEditControl.Value?.Reaction)
			{
				ResumeMonitor();

				_MonitorModel.StopMonitoring(reaction);

				ReactionEditControl.Value = new ReactionEditControlViewModel(this, _RegionManager, _MonitorModel, AppPolicyManager, reaction);


				foreach(var item in ReactionItems.Where(x => x.Reaction != reaction))
				{
					item.IsSelected = false;
				}

				var reac = ReactionItems.SingleOrDefault(x => x.Reaction == reaction);
				if (reac != null)
				{
					reac.IsSelected = true;
				}

			}
		}

		private void ResumeMonitor()
		{
			if (ReactionEditControl.Value != null)
			{
				ReactionEditControl.Value.Save();

				_MonitorModel.StartMonitoring(ReactionEditControl.Value.Reaction);

				ReactionEditControl.Value.Dispose();
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



		internal async void DeleteFolderReactionFile(FolderReactionModel reaction)
		{
			// 確認ダイアログを表示
			var result = await ShowReactionDeleteConfirmDialog();

			// 削除
			if (result != null && result.HasValue && result.Value == true)
			{
				_MonitorModel.StopMonitoring(reaction);

				var reactionSaveFoler = _MonitorModel.FindReactionParentFolder(reaction);
				reactionSaveFoler.RemoveReaction(reaction.Guid);


				var currentReaction = this.ReactionEditControl.Value?.Reaction;
				if (currentReaction == reaction)
				{
					this.ReactionEditControl.Value = null;
				}

			}
		}

		public async Task<bool?> ShowReactionDeleteConfirmDialog()
		{
			var view = new Views.DialogContent.DeleteReactionConfirmDialogContent();

			return (bool?)await DialogHost.Show(view, "ReactionEditCommonDialogHost");
		}
	}





}
