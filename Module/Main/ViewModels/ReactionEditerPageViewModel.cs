using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Modules.Main.ViewModels.ReactionEditer;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.AppPolicy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels
{

	public enum ReactionFilterType
	{
		Files,
		Folder,

		Unknown
	}

	// TODO: WorkFolderの切り替えに伴うVMの変更部分について、ViewModelをさらに切り出して読みやすくしたい
	
	public class ReactionEditerPageViewModel : PageViewModelBase, INavigationAware, IDisposable
	{
		private FolderReactionModel Reaction;
		private IAppPolicyManager _AppPolicyManager;
		private CompositeDisposable _CompositeDisposable;

		public IRegionNavigationService NavigationService;


		public bool IsReactionValid { get; private set; }

		public ReactiveProperty<string> ReactionWorkName { get; private set; }

		public ReactiveProperty<WorkFolderEditViewModel> WorkFolderEditVM { get; private set; }
		public ReactiveProperty<FilterEditViewModel> FilterEditVM { get; private set; }
		public ReactiveProperty<ActionsEditViewModel> ActionsEditVM { get; private set; }
		public ReactiveProperty<DestinationEditViewModel> DestinationEditVM { get; private set; }

		public ReactiveProperty<bool> IsEnable { get; private set; }

		public ReactiveProperty<bool> CanSave { get; private set; }


		public ReactionEditerPageViewModel(IRegionManager regionManager, IFolderReactionMonitorModel monitor, IAppPolicyManager appPolicyManager)
			: base(regionManager, monitor)
		{
			_AppPolicyManager = appPolicyManager;

			IsReactionValid = false;

			ReactionWorkName = new ReactiveProperty<string>("");

			WorkFolderEditVM = new ReactiveProperty<WorkFolderEditViewModel>();
			FilterEditVM = new ReactiveProperty<FilterEditViewModel>();
			ActionsEditVM = new ReactiveProperty<ActionsEditViewModel>();
			DestinationEditVM = new ReactiveProperty<DestinationEditViewModel>();

			IsEnable = new ReactiveProperty<bool>(false);

			CanSave = new ReactiveProperty<bool>(true);
		}


		private void Initialize()
		{
			// initialize with this.Reaction
			CleanUpReactionSubscribe();

			_CompositeDisposable = new CompositeDisposable();

			ReactionWorkName.Value = Reaction.Name;

			Reaction.ObserveProperty(x => x.WorkFolder)
				.Subscribe(x =>
				{
					WorkFolderEditVM.Value?.Dispose();
					FilterEditVM.Value?.Dispose();
					ActionsEditVM.Value?.Dispose();
					DestinationEditVM.Value?.Dispose();

					WorkFolderEditVM.Value = new WorkFolderEditViewModel(Reaction);
					FilterEditVM.Value = new FilterEditViewModel(Reaction);
					ActionsEditVM.Value = new ActionsEditViewModel(Reaction, _AppPolicyManager);
					DestinationEditVM.Value = new DestinationEditViewModel(Reaction);
				})
				.AddTo(_CompositeDisposable);

			Reaction.ObserveProperty(x => x.IsValid)
				.Subscribe(x =>
				{
					IsReactionValid = x;
					OnPropertyChanged(nameof(IsReactionValid));
				})
				.AddTo(_CompositeDisposable);

			// On/Off
			IsEnable.Value = Reaction.IsEnable;
			IsEnable.Subscribe(x =>
				{
					Reaction.IsEnable = x;
				})
				.AddTo(_CompositeDisposable);


			CanSave.Value = true;
		}


		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{

		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			NavigationService = navigationContext.NavigationService;
			Reaction = base.ReactionModelFromNavigationParameters(navigationContext.Parameters);

			Initialize();
		}


		private void CleanUpReactionSubscribe()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}

		public void Dispose()
		{
			CleanUpReactionSubscribe();

			ReactionWorkName?.Dispose();
			CanSave?.Dispose();

			WorkFolderEditVM.Value?.Dispose();
			FilterEditVM.Value?.Dispose();
			ActionsEditVM.Value?.Dispose();
			DestinationEditVM.Value?.Dispose();
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
							this.NavigationToFolderListPage(_MonitorModel.RootFolder);
						}
					}));
			}
		}



		private DelegateCommand _SaveCommand;
		public DelegateCommand SaveCommand
		{
			get
			{
				return _SaveCommand
					?? (_SaveCommand = new DelegateCommand(() =>
					{
						CanSave.Value = false;

						try
						{
							Task.Run(async () =>
							{
								_MonitorModel.SaveReaction(Reaction);
								await Task.Delay(500);
							})
							.ContinueWith(_ => { CanSave.Value = true; });
						}
						finally
						{

						}
					}));
			}
		}

		private DelegateCommand _TestCommand;
		public DelegateCommand TestCommand
		{
			get
			{
				return _TestCommand
					?? (_TestCommand = new DelegateCommand(() =>
					{
						Reaction.Test();
//						Reaction.Start();
//						Reaction.CheckNow();
					}
					));
			}
		}

		private DelegateCommand _ExportCommand;
		public DelegateCommand ExportCommand
		{
			get
			{
				return _ExportCommand
					?? (_ExportCommand = new DelegateCommand(() =>
					{
						// SaveFileDialogで保存先パスを取得

						// ファイルコピー
						// 出力先のファイル名を取得
						var dialog = new SaveFileDialog();

						dialog.Title = "ReactiveFolder - select export application policy file";
						dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

						dialog.AddExtension = true;
						dialog.FileName = Reaction.Name;
						dialog.Filter = $"Json|*.json|All|*.*";


						var result = dialog.ShowDialog();


						if (result != null && ((bool)result) == true)
						{
							var destFilePath = dialog.FileName;

							var reactionSaveFoler = _MonitorModel.FindReactionParentFolder(Reaction);
							var sourceFilePath = reactionSaveFoler.MakeReactionSaveFilePath(Reaction);

							var sourceFileInfo = new FileInfo(sourceFilePath);

							try
							{
								sourceFileInfo.CopyTo(destFilePath);
							}
							catch
							{
								System.Diagnostics.Debug.WriteLine("failed export Reaction File.");
								System.Diagnostics.Debug.WriteLine("from :" + sourceFilePath);
								System.Diagnostics.Debug.WriteLine("  to :" + destFilePath);

								throw;
							}

							// TODO: エクスポート完了のトースト表示
							// 出力先フォルダをトーストから開けるとよりモアベター
						}
					}
					));
			}
		}

		private DelegateCommand _DeleteCommand;
		public DelegateCommand DeleteCommand
		{
			get
			{
				return _DeleteCommand
					?? (_DeleteCommand = new DelegateCommand(async () =>
					{
						// 確認ダイアログを表示
						var result = await ShowReactionDeleteConfirmDialog();

						// 削除
						if (result != null && result.HasValue && result.Value == true)
						{
							var reactionSaveFoler = _MonitorModel.FindReactionParentFolder(Reaction);
							reactionSaveFoler.RemoveReaction(Reaction.Guid);

							await BackCommand.Execute();
						}
					}
					));
			}
		}



		public async Task<bool?> ShowReactionDeleteConfirmDialog()
		{
			var view = new Views.DialogContent.DeleteReactionConfirmDialogContent();

			return (bool?)await DialogHost.Show(view, "ReactionEditCommonDialogHost");
		}

	}
}
