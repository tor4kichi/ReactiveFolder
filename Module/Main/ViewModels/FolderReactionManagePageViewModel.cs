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
using ReactiveFolder.Models.History;
using ReactiveFolderStyles.Models;
using ReactiveFolderStyles.ViewModels;

namespace Modules.Main.ViewModels
{



	// TODO: Stack構造によるフォルダーの階層構造を表現するUIを作成する




	public class FolderReactionManagePageViewModel : PageViewModelBase
	{
		public IFolderReactionMonitorModel Monitor { get; private set; }
		private IEventAggregator _EventAggregator;
		public IAppPolicyManager AppPolicyManager { get; private set; }
		public IHistoryManager HistoryManager { get; private set; }

		public FolderModel CurrentFolder { get; private set; }

		public ReadOnlyReactiveCollection<ReactionListItemViewModel> ReactionItems { get; private set; }

		public ReadOnlyReactiveCollection<FolderListItemViewModel> FolderItems { get; private set; }

		public string PreviousFolderName { get; private set; }

		public ReactiveProperty<string> FolderName { get; private set; }



//		public ReactiveProperty<ReactionEditPageViewModel> ReactionEditControl { get; private set; }

		public FolderReactionManagePageViewModel(PageManager pageManager, IFolderReactionMonitorModel monitor, IEventAggregator ea, IAppPolicyManager appPolicyManager, IHistoryManager historyManager)
			: base(pageManager)
		{
			Monitor = monitor;
			_EventAggregator = ea;
			AppPolicyManager = appPolicyManager;
			HistoryManager = historyManager;

			FolderName = new ReactiveProperty<string>("");
			/*
			CurrentFolder = _MonitorModel.RootFolder;

			ReactionListItems = CurrentFolder.Models
				.ToReadOnlyReactiveCollection(x => new ReactionListItemViewModel(this, x));

			ChildrenFolderListItems = CurrentFolder.Children
				.ToReadOnlyReactiveCollection(x => new FolderListItemViewModel(this, x));
				*/
			PreviousFolderName = "";


//			ReactionEditControl = new ReactiveProperty<ReactionEditPageViewModel>();

			Initialize(Monitor.RootFolder);
		}


		public override void OnNavigatedFrom(NavigationContext navigationContext)
		{
			
		}

		public async Task<bool?> ShowReactionEditCloseConfirmDialog()
		{
			var view = new Views.DialogContent.ReactionSaveAndBackConfirmDialogContent();

			return (bool?)await DialogHost.Show(view, "ReactionEditCommonDialogHost");
		}

		public override void OnNavigatedTo(NavigationContext navigationContext)
		{
			Initialize(Monitor.RootFolder);


			if (navigationContext.Parameters.Count() > 0)
			{
				if (navigationContext.Parameters.Any(x => x.Key == "guid"))
				{
					try
					{
						var reactionGuid = (Guid)navigationContext.Parameters["guid"];

						SelectedReaction(Monitor.RootFolder.FindReaction(reactionGuid));
					}
					catch
					{
						Console.WriteLine("FolderReactionManagePage: パラメータが不正です。存在するReactionのGuidを指定してください。");
					}
				}

				else if (navigationContext.Parameters.Any(x => x.Key == "filepath"))
				{
					try
					{
						var reactionFilePath = (string)navigationContext.Parameters["filepath"];

						var reaction = Monitor.RootFolder.Reactions
							.SingleOrDefault(x => Monitor.RootFolder.MakeReactionSaveFilePath(x) == reactionFilePath);


						if (reaction == null)
						{
							throw new Exception("use import reaction.");
						}

						SelectedReaction(reaction);
					}
					catch
					{
						Console.WriteLine("FolderReactionManagePage: パラメータが不正です。存在するReactionのGuidを指定してください。");
					}
				}}
			
		}



		public static NavigationParameters CreateOpenReactionParameter(Guid reactionGuid)
		{
			var parameters = new NavigationParameters();

			parameters.Add("guid", reactionGuid);
			return parameters;
		}

		public static NavigationParameters CreateOpenReactionParameter(string filePath)
		{
			var parameters = new NavigationParameters();

			parameters.Add("filepath", filePath);
			return parameters;
		}

		private void Initialize(FolderModel folder)
		{
			CurrentFolder = folder;

			FolderName.Value = CurrentFolder.Folder.Name;

			ReactionItems = CurrentFolder.Reactions
				.ToReadOnlyReactiveCollection(x => new ReactionListItemViewModel(this, x));

			FolderItems = CurrentFolder.Children
				.ToReadOnlyReactiveCollection(x => new FolderListItemViewModel(this, x));

			if (folder == Monitor.RootFolder)
			{
				// ルートは戻る無効
				PreviousFolderName = "";
				OnPropertyChanged(nameof(PreviousFolderName));
			}
			else
			{
				var parentFolder = Path.GetDirectoryName(folder.Folder.FullName);
				PreviousFolderName = Path.GetFileName(parentFolder);
				OnPropertyChanged(nameof(PreviousFolderName));
			}



		}


		internal void SelectedReaction(FolderReactionModel reaction)
		{
			var reactionVM = ReactionItems.SingleOrDefault(x => x.Reaction == reaction);
			if (reactionVM != null)
			{
				reactionVM.IsSelected = true;
			}
		}


		internal void ShowReaction(FolderReactionModel reaction)
		{
			PageManager.OpenReaction(reaction.Guid);

			SelectedReaction(reaction);
		}
		/*

		private void SetupEdit(FolderReactionModel reaction)
		{
			if (reaction != ReactionEditControl.Value?.Reaction)
			{
				ExitEdit();

				// リアクションのモニタリングを止める
				_MonitorModel.StopMonitoring(reaction);


				// VMを設定
				ReactionEditControl.Value = new ReactionEditPageViewModel(this, _RegionManager, _MonitorModel, AppPolicyManager, HistoryManager, reaction);


				// リスト表示を更新
				var reac = ReactionItems.SingleOrDefault(x => x.Reaction == reaction);
				if (reac != null)
				{
					reac.IsSelected = true;
				}
			}

		}

		private void ExitEdit()
		{
			if (ReactionEditControl.Value != null)
			{
				var reaction = ReactionEditControl.Value.Reaction;

				ReactionEditControl.Value.Save();


				// リスト表示を更新
				foreach (var item in ReactionItems)
				{
					item.IsSelected = false;
				}


				// VMを解放
				ReactionEditControl.Value.Dispose();
				ReactionEditControl.Value = null;

				// リアクションのモニタリングを再開させる
				_MonitorModel.StartMonitoring(reaction);
			}
		}




	*/

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

						// TODO: FolderStackにフォルダを積む
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

		private FolderReactionModel ImportReactionFile(string path)
		{
			var importedReaction = FileSerializeHelper.LoadAsync<FolderReactionModel>(path);

			if (null != Monitor.FindReaction(importedReaction.Guid))
			{
				// alread exist reaction
				// Guidを張り替える？
			}
			else
			{
				CurrentFolder.AddReaction(importedReaction);
			}

			return importedReaction;
		}




		

		private DelegateCommand _RemoveThisFolderCommand;
		public DelegateCommand RemoveThisFolderCommand
		{
			get
			{
				return _RemoveThisFolderCommand
					?? (_RemoveThisFolderCommand = new DelegateCommand(() =>
					{
						/*
						if (CurrentFolder.Children.Count > 0)
						{
							// confirm delete dialog.
						}

						
						_MonitorModel.RootFolder.RemoveFolder(CurrentFolder);
						*/

					}
					, () => Monitor.RootFolder.Folder.FullName != this.CurrentFolder.Folder.FullName
					
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
				Monitor.StopMonitoring(reaction);

				var reactionSaveFoler = Monitor.FindReactionParentFolder(reaction);
				reactionSaveFoler.RemoveReaction(reaction.Guid);
			}
		}

		public async Task<bool?> ShowReactionDeleteConfirmDialog()
		{
			var view = new Views.DialogContent.DeleteReactionConfirmDialogContent();

			return (bool?)await DialogHost.Show(view, "ReactionEditCommonDialogHost");
		}
	}





}
