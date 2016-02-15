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
using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

		public ReactiveProperty<ReactionViewModel> ReactionVM { get; private set; }

		public string RollbackData { get; private set; }


		public ReactiveProperty<bool> IsNeedSave { get; private set; }

		private IDisposable CanSaveSubscriber;

		public ReactionEditerPageViewModel(IRegionManager regionManager, IFolderReactionMonitorModel monitor, IAppPolicyManager appPolicyManager)
			: base(regionManager, monitor)
		{
			_AppPolicyManager = appPolicyManager;
			_CompositeDisposable = new CompositeDisposable();

			ReactionVM = new ReactiveProperty<ReactionViewModel>();
			IsNeedSave = new ReactiveProperty<bool>(false);

			SaveCommand = IsNeedSave.ToReactiveCommand(false)
					.AddTo(_CompositeDisposable);

			SaveCommand.Subscribe(_ => Save())
				.AddTo(_CompositeDisposable);
		}


		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
			// 削除されている場合を除いて
			if (this.Reaction != null)
			{
				// 一時停止していたモニタリングを再開
				_MonitorModel.StartMonitoring(this.Reaction);
			}
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			NavigationService = navigationContext.NavigationService;

			var reaction = base.ReactionModelFromNavigationParameters(navigationContext.Parameters);

			if (Reaction != reaction)
			{
				ReactionVM.Value?.Dispose();

				Reaction = reaction;
				RollbackData = FileSerializeHelper.ToJson(Reaction);

				ReactionVM.Value = new ReactionViewModel(Reaction, _AppPolicyManager);

				IsNeedSave.Value = false;

				CanSaveSubscriber = Observable.Merge(
					Reaction.ObserveProperty(x => x.IsNeedValidation, false).ToUnit(),
					Reaction.PropertyChangedAsObservable().ToUnit()
					)
					.Subscribe(_ =>
					{
						IsNeedSave.Value = true;
					});



				// モニター処理を一時停止
				_MonitorModel.StopMonitoring(this.Reaction);

			}
		}

		public void Dispose()
		{
			ReactionVM.Value?.Dispose();
			CanSaveSubscriber?.Dispose();
			_CompositeDisposable?.Dispose();
		}

		private DelegateCommand _BackCommand;
		public DelegateCommand BackCommand
		{
			get
			{
				return _BackCommand
					?? (_BackCommand = new DelegateCommand(async () =>
					{
						if (IsNeedSave.Value)
						{
							var result = await ShowReactionSaveAndBackConfirmDialog();

							if (result.HasValue == false)
							{
								// キャンセル
							}
							else if (result.Value == true)
							{
								// 保存して戻る
								Save();
								Back();
							}
							else if (result.Value == false)
							{
								// 保存せずに戻る

								// ロールバックして戻る
								RollbackReactionModel();
								Back();
							}
						}
						else
						{
							Back();
						}

						
					}));
			}
		}



		private void Back()
		{
			if (NavigationService.Journal.CanGoBack)
			{
				NavigationService.Journal.GoBack();
			}
			else
			{
				this.NavigationToFolderListPage(_MonitorModel.RootFolder);
			}
		}

		private void RollbackReactionModel()
		{
			var prevModel = FileSerializeHelper.FromJson<FolderReactionModel>(RollbackData);

			Reaction.WorkFolder = prevModel.WorkFolder;
			Reaction.Filter = prevModel.Filter;

			var remActions = Reaction.Actions.ToList();
			foreach (var remAction in remActions)
			{
				Reaction.RemoveAction(remAction);
			}

			foreach (var action in prevModel.Actions)
			{
				Reaction.AddAction(action);
			}

			// Timingsは
			//			Reaction.FileUpdateTiming = prevModel.FileUpdateTiming;

			Reaction.Destination.OutputNamePattern = prevModel.Destination.OutputNamePattern;
			Reaction.Destination.AbsoluteFolderPath = prevModel.Destination.AbsoluteFolderPath;


			IsNeedSave.Value = false;
		}


		public ReactiveCommand SaveCommand { get; private set; }

		private void Save()
		{
			IsNeedSave.Value = false;

			try
			{
				_MonitorModel.SaveReaction(Reaction);
			}
			catch
			{
				IsNeedSave.Value = true;
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
						Reaction.Execute();
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
							_MonitorModel.StopMonitoring(Reaction);

							var reactionSaveFoler = _MonitorModel.FindReactionParentFolder(Reaction);
							reactionSaveFoler.RemoveReaction(Reaction.Guid);


							Reaction = null;

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

		public async Task<bool?> ShowReactionSaveAndBackConfirmDialog()
		{
			var view = new Views.DialogContent.ReactionSaveAndBackConfirmDialogContent();

			return (bool?)await DialogHost.Show(view, "ReactionEditCommonDialogHost");
		}

	}



	public class ReactionViewModel : BindableBase, IDisposable
	{
		CompositeDisposable _CompositeDisposable;


		FolderReactionModel Reaction;
		IAppPolicyManager _AppPolicyManager;

		public ReactiveProperty<bool> IsReactionValid { get; private set; }

		public ReactiveProperty<string> ReactionWorkName { get; private set; }

		public WorkFolderEditViewModel WorkFolderEditVM { get; private set; }
		public FilterEditViewModel FilterEditVM { get; private set; }
		public ActionsEditViewModel ActionsEditVM { get; private set; }
		public DestinationEditViewModel DestinationEditVM { get; private set; }

		public ReactiveProperty<bool> IsEnable { get; private set; }
		public ReactiveProperty<string> MonitorIntervalSeconds { get; private set; }

		// Reactionmodelを受け取ってVMを生成する

		public ReactionViewModel(FolderReactionModel reaction, IAppPolicyManager appPolicyManager)
		{
			_CompositeDisposable = new CompositeDisposable();
			Reaction = reaction;
			_AppPolicyManager = appPolicyManager;


			IsReactionValid = Reaction.ObserveProperty(x => x.IsValid)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			ReactionWorkName = Reaction.ToReactivePropertyAsSynchronized(x => x.Name)
				.AddTo(_CompositeDisposable);

			WorkFolderEditVM = new WorkFolderEditViewModel(Reaction);
			FilterEditVM = new FilterEditViewModel(Reaction);
			ActionsEditVM = new ActionsEditViewModel(Reaction, _AppPolicyManager);
			DestinationEditVM = new DestinationEditViewModel(Reaction);

			IsEnable = Reaction.ToReactivePropertyAsSynchronized(x => x.IsEnable)
				.AddTo(_CompositeDisposable);

			// see@ http://stackoverflow.com/questions/1833830/timespan-parse-time-format-hhmmss
			// https://msdn.microsoft.com/en-us/library/ee372286.aspx
			MonitorIntervalSeconds = Reaction.ToReactivePropertyAsSynchronized(
				x => x.CheckInterval
				, convert: (timespan) => ((int)timespan.TotalSeconds).ToString()
				, convertBack: (seconds) => TimeSpan.FromSeconds(int.Parse(seconds))
				, ignoreValidationErrorValue: true
			)
			.AddTo(_CompositeDisposable);

			MonitorIntervalSeconds.SetValidateNotifyError(text =>
			{
				int temp;
				if (false == int.TryParse(text, out temp))
				{
					return "Number Only";
				}

				return null;
			});

			


		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
		}
	}
}
