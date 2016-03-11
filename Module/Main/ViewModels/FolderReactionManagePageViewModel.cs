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
using System.Collections.ObjectModel;

namespace Modules.Main.ViewModels
{



	// TODO: Stack構造によるフォルダーの階層構造を表現するUIを作成する




	public class FolderReactionManagePageViewModel : PageViewModelBase
	{
		public IFolderReactionMonitorModel Monitor { get; private set; }
		private IEventAggregator _EventAggregator;
		public IAppPolicyManager AppPolicyManager { get; private set; }
		public IHistoryManager HistoryManager { get; private set; }

		public IReactiveFolderSettings Settings { get; private set; }

		
		public ObservableCollection<ReactionManageFolderViewModel> FolderStack { get; private set; }

		public ReactiveProperty<ReactionManageFolderViewModel> CurrentFolder { get; private set; }

		public FolderReactionManagePageViewModel(PageManager pageManager, IFolderReactionMonitorModel monitor, IEventAggregator ea, IAppPolicyManager appPolicyManager, IHistoryManager historyManager, IReactiveFolderSettings settings)
			: base(pageManager)
		{
			Monitor = monitor;
			_EventAggregator = ea;
			AppPolicyManager = appPolicyManager;
			HistoryManager = historyManager;
			Settings = settings;



			CurrentFolder = new ReactiveProperty<ReactionManageFolderViewModel>();
			FolderStack = new ObservableCollection<ReactionManageFolderViewModel>();

			FolderStack.CollectionChangedAsObservable()
				.Subscribe(x => 
				{
					CurrentFolder.Value = FolderStack.Last();
				});

			FolderStack.Add(new ReactionManageFolderViewModel(this, Monitor.RootFolder));
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


			if (navigationContext.Parameters.Count() > 0)
			{
				if (navigationContext.Parameters.Any(x => x.Key == "guid"))
				{
					try
					{
//						var reactionGuid = (Guid)navigationContext.Parameters["guid"];

//						SelectReaction(Monitor.RootFolder.FindReaction(reactionGuid));
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

//						SelectReaction(reaction);
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



		internal void SelectReaction(FolderReactionModel reaction)
		{
			var reactionParentFolder = Monitor.FindReactionParentFolder(reaction);

			SelectFolder(reactionParentFolder);


			var reactionListItem = CurrentFolder.Value.ReactionItems.SingleOrDefault(x => x.Reaction == reaction);

			if (reactionListItem != null)
			{
				CurrentFolder.Value.SelectReaction(reactionListItem);
			}
		}


		internal void ShowReaction(FolderReactionModel reaction)
		{
			PageManager.OpenReaction(reaction.Guid);
		}



		internal void SelectFolder(FolderModel folder)
		{
			if (CurrentFolder?.Value.Folder == folder)
			{
				return;
			}

			// 一個上のフォルダを積むとき
			if (CurrentFolder.Value.Folder.Children.Any(x => x == folder))
			{
				var folderVM = new ReactionManageFolderViewModel(this, folder);
				FolderStack.Add(folderVM);

				return;
			}


			
			var existFolder = FolderStack.SingleOrDefault(x => x.Folder == folder);


			// 前のフォルダに戻る時
			if (existFolder != null)
			{
				var position = FolderStack.IndexOf(existFolder);

				position += 1;

				var removeFolders = FolderStack.Where((x, index) => position <= index).ToArray();
				foreach (var removeFolder in removeFolders)
				{
					FolderStack.Remove(removeFolder);
				}
			}

			// それ以外の時は一旦全削除して再度フォルダを積み直す
			else
			{
				FolderStack.Clear();

				var folderStackModels = folder.GetAllParent();

				folderStackModels.Add(folder);

				foreach (var parentFolder in folderStackModels)
				{
					FolderStack.Add(new ReactionManageFolderViewModel(this, parentFolder));
				}
			}
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
						CurrentFolder.Value.Folder.UpdateReactionModels();
						CurrentFolder.Value.Folder.UpdateChildren();
					}));
			}
		}


		


		

		internal FolderReactionModel ImportReactionFile(FolderModel folder, string path)
		{
			var importedReaction = FileSerializeHelper.LoadAsync<FolderReactionModel>(path);

			if (null != Monitor.FindReaction(importedReaction.Guid))
			{
				// alread exist reaction
				// Guidを張り替える？
			}
			else
			{
				folder.AddReaction(importedReaction);
			}

			return importedReaction;
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

						var folderModel = CurrentFolder.Value.Folder.AddFolder(newFolderName);

						SelectFolder(folderModel);
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

						reaction.CheckInterval = TimeSpan.FromSeconds(Settings.DefaultMonitorIntervalSeconds);

						// AddReaction中で非同期での保存処理が走る
						var currentFolder = CurrentFolder.Value.Folder;
						Task.Run(() =>
						{
							currentFolder.AddReaction(reaction);
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
							var currentFolder = CurrentFolder.Value.Folder;
							foreach (var destFilePath in dialog.FileNames)
							{
								ImportReactionFile(currentFolder, destFilePath);
							}
						}

						// move to Reaction editer page.
						//						NavigationToReactionEditerPage(reaction);
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
						/*
						if (CurrentFolder.Children.Count > 0)
						{
							// confirm delete dialog.
						}

						
						_MonitorModel.RootFolder.RemoveFolder(CurrentFolder);
						*/

					}
					, () => FolderStack.Count >= 1
					
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
//						Process.Start(this.CurrentFolder.Folder.FullName);
					}));
			}
		}



		internal async Task<bool> DeleteFolderReactionFile(FolderReactionModel reaction)
		{
			// 確認ダイアログを表示
			var result = await ShowReactionDeleteConfirmDialog();

			// 削除
			if (result != null && result.HasValue && result.Value == true)
			{
				Monitor.StopMonitoring(reaction);

				var reactionSaveFoler = Monitor.FindReactionParentFolder(reaction);
				reactionSaveFoler.RemoveReaction(reaction.Guid);

				return true;
			}
			else
			{
				return false;
			}

			
		}

		public async Task<bool?> ShowReactionDeleteConfirmDialog()
		{
			var view = new Views.DialogContent.DeleteReactionConfirmDialogContent();

			return (bool?)await DialogHost.Show(view, "ReactionManageCommonDialogHost");
		}
	}


	public class ReactionManageFolderViewModel : BindableBase
	{
		public FolderReactionManagePageViewModel PageVM { get; private set; }

		public FolderModel Folder { get; private set; }

		public string FolderName { get; private set; }

		public ReadOnlyReactiveCollection<ReactionListItemViewModel> ReactionItems { get; private set; }

		public ReadOnlyReactiveCollection<FolderListItemViewModel> FolderItems { get; private set; }

		public ReactiveProperty<ReactionListItemViewModel> SelectedReaction { get; private set; }


		public ReactionManageFolderViewModel(FolderReactionManagePageViewModel pageVM, FolderModel folder)
		{
			PageVM = pageVM;
			Folder = folder;
			FolderName = Folder.Name;

			ReactionItems = Folder.Reactions
				.ToReadOnlyReactiveCollection(x => new ReactionListItemViewModel(PageVM, x));

			FolderItems = Folder.Children
				.ToReadOnlyReactiveCollection(x => new FolderListItemViewModel(PageVM, x));
			
			SelectedReaction = new ReactiveProperty<ReactionListItemViewModel>();

			SelectedReaction.Subscribe(x =>
			{
				if (x != null)
				{
					PageVM.ShowReaction(x.Reaction);
					x.IsSelected = true;
				}

				foreach (var item in ReactionItems.Where(y => y != x))
				{
					item.IsSelected = false;
				}

			});
			
		}

		private DelegateCommand _SelectFolderCommand;
		public DelegateCommand SelectFolderCommand
		{
			get
			{
				return _SelectFolderCommand
					?? (_SelectFolderCommand = new DelegateCommand(() =>
					{
						PageVM.SelectFolder(Folder);
					}));
			}
		}


		internal void SelectReaction(ReactionListItemViewModel reactionListItem)
		{
			SelectedReaction.Value = reactionListItem;
		}



	}


}
