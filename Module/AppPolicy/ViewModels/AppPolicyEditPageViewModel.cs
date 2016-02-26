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
using ReactiveFolder.Models.AppPolicy;
using MaterialDesignThemes.Wpf;
using System.Reactive.Linq;
using ReactiveFolder.Models.Util;
using Microsoft.Win32;
using ReactiveFolderStyles.DialogContent;
using System.Reactive.Disposables;

namespace Modules.AppPolicy.ViewModels
{
	public class AppPolicyEditPageViewModel : PageViewModelBase, INavigationAware, IDisposable
	{
		public IRegionNavigationService NavigationService;

		public ApplicationPolicy AppPolicy { get; private set; }
		public ReactiveProperty<ApplicationPolicyViewModel> AppPolicyVM { get; private set; }


		public string RollbackData { get; private set; }


		public ReactiveProperty<bool> IsNeedSave { get; private set; }

		private IDisposable CanSaveSubscriber;

		public AppPolicyEditPageViewModel(IRegionManager regionManager, IAppPolicyManager appPolicyManager)
			: base(regionManager, appPolicyManager)
		{
			AppPolicyVM = new ReactiveProperty<ApplicationPolicyViewModel>();
			IsNeedSave = new ReactiveProperty<bool>(false);

			SaveCommand = IsNeedSave.ToReactiveCommand(false);

			SaveCommand.Subscribe(_ => Save());
		}


		public void Dispose()
		{
			AppPolicyVM?.Dispose();
			IsNeedSave?.Dispose();
			CanSaveSubscriber?.Dispose();
			SaveCommand?.Dispose();
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

			try
			{
				AppPolicyVM.Value?.Dispose();
				CanSaveSubscriber?.Dispose();


				var policy = base.ApplicationPolicyFromNavigationParameters(navigationContext.Parameters);

				AppPolicy = policy;
				AppPolicyVM.Value = new ApplicationPolicyViewModel(this, _AppPolicyManager, policy);

				BackupAppPolicyModel();

				IsNeedSave.Value = false;

				CanSaveSubscriber = Observable.Merge(
						AppPolicy.PropertyChangedAsObservable().ToUnit(),
						AppPolicy.AcceptExtentions.CollectionChangedAsObservable().ToUnit(),
						AppPolicy.OptionDeclarations.CollectionChangedAsObservable().ToUnit(),
						AppPolicy.OptionDeclarations.ObserveElementPropertyChanged().ToUnit()
					)
					.Subscribe(_ =>
					{
						IsNeedSave.Value = true;
					});
			}
			catch
			{
				NavigationService.Journal.GoBack();
			}
		}



		private void BackupAppPolicyModel()
		{
			RollbackData = FileSerializeHelper.ToJson(AppPolicy);
		}

		private void RollbackAppPolicyModel()
		{
			var prevModel = FileSerializeHelper.FromJson<ApplicationPolicy>(RollbackData);

			AppPolicy.RollbackFrom(prevModel);

			IsNeedSave.Value = false;
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
							var result = await ShowAppPolicySaveAndBackConfirmDialog();

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
								RollbackAppPolicyModel();
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
				_RegionManager.RequestNavigate("MainRegion", "FolderListPage");
			}
		}


		public ReactiveCommand SaveCommand { get; private set; }

		private void Save()
		{
			IsNeedSave.Value = false;

			try
			{
				_AppPolicyManager.SavePolicyFile(this.AppPolicy);	
			}
			catch
			{
				IsNeedSave.Value = true;
			}

			BackupAppPolicyModel();
		}


		private DelegateCommand _ExportCommand;
		public DelegateCommand ExportCommand
		{
			get
			{
				return _ExportCommand
					?? (_ExportCommand = new DelegateCommand(() =>
					{
						var appPolicy = this.AppPolicyVM.Value.AppPolicy;
						// 出力先のファイル名を取得
						var dialog = new SaveFileDialog();

						dialog.Title = "ReactiveFolder - select export application policy file";
						dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

						dialog.AddExtension = true;
						dialog.FileName = appPolicy.AppName;
						dialog.Filter = $"App Policy|*{_AppPolicyManager.PolicyFileExtention}|Json|*.json|All|*.*";
						dialog.DefaultExt = _AppPolicyManager.PolicyFileExtention;


						var result = dialog.ShowDialog();


						if (result != null && ((bool)result) == true)
						{
							var destFilePath = dialog.FileName;
							var destFileInfo = new FileInfo(destFilePath);

							var sourceFilePath = _AppPolicyManager.GetSaveFilePath(appPolicy);

							var sourceFileInfo = new FileInfo(sourceFilePath);

							
							try
							{
								if (destFileInfo.Exists)
								{
									destFileInfo.Delete();
								}

								sourceFileInfo.CopyTo(destFilePath);
							}
							catch
							{
								System.Diagnostics.Debug.WriteLine("failed export Application Policy.");
								System.Diagnostics.Debug.WriteLine("from :" + sourceFilePath);
								System.Diagnostics.Debug.WriteLine("  to :" + destFilePath);
							}
						}
						


						// 
					}));
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
						var result = await ShowAppPolicyDeleteConfirmDialog();

						if (result != null && ((bool)result) == true)
						{
							_AppPolicyManager.RemoveAppPolicy(this.AppPolicyVM.Value.AppPolicy);

							NavigationService.Journal.GoBack();
						}
					}));
			}
		}

		
		public async Task<bool?> ShowAppPolicySaveAndBackConfirmDialog()
		{
			var view = new SaveAndBackConfirmDialogContent();

			return (bool?)await DialogHost.Show(view, "AppPolicyEditDialogHost");
		}

		public async Task<bool?> ShowAppPolicyDeleteConfirmDialog()
		{
			var view = new DeleteConfirmDialogContent()
			{
				DataContext = new DeleteConfirmDialogContentViewModel()
				{
					Title = "Delete AppPolicy?"
				}
			};



			return (bool?)await DialogHost.Show(view, "AppPolicyEditDialogHost");
		}

		
	}


	public class ApplicationPolicyViewModel : BindableBase, IDisposable
	{
		public static FolderItemType[] FolderItemTypes = (FolderItemType[]) Enum.GetValues(typeof(FolderItemType));

		private CompositeDisposable _CompositeDisposable;

		public AppPolicyEditPageViewModel EditPageVM { get; private set; }
		public IAppPolicyManager AppPolicyManager { get; private set; }
		public ApplicationPolicy AppPolicy { get; private set; }

		public ReactiveProperty<string> ApplicationPath { get; private set; }

		public ReactiveProperty<string> AppName { get; private set; }



		public ReactiveProperty<FolderItemType> InputPathType { get; private set; }

		public ReactiveProperty<bool> IsInputFile { get; private set; }



		public ReactiveProperty<string> ExtentionText { get; private set; }

		// 入力可能なファイル拡張子（複数）
		public ReadOnlyReactiveCollection<string> AcceptExtentions { get; private set; }


		// オプションのプロトタイプ宣言
		public AppOptionDeclarationViewModel InputDeclaration { get; private set; }
		public ReadOnlyReactiveCollection<AppOptionDeclarationViewModel> OutputOptionDeclarations { get; private set; }
		public ReadOnlyReactiveCollection<AppOptionDeclarationViewModel> OptionDeclarations { get; private set; }


		public ReactiveProperty<ApplicationPathState> AppPathState { get; private set; }


		public ApplicationPolicyViewModel(AppPolicyEditPageViewModel editPageVM, IAppPolicyManager manager, ApplicationPolicy appPolicy)
		{
			EditPageVM = editPageVM;
			AppPolicyManager = manager;
			AppPolicy = appPolicy;

			_CompositeDisposable = new CompositeDisposable();

			ApplicationPath = AppPolicy.ObserveProperty(x => x.ApplicationPath)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			AppName = AppPolicy.ToReactivePropertyAsSynchronized(x => x.AppName)
				.AddTo(_CompositeDisposable);


			InputPathType = AppPolicy.ToReactivePropertyAsSynchronized(x => x.InputPathType)
				.AddTo(_CompositeDisposable);

			IsInputFile = InputPathType.Select(x => x == FolderItemType.File)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);



			ExtentionText = new ReactiveProperty<string>("")
				.AddTo(_CompositeDisposable);

			AcceptExtentions = appPolicy.AcceptExtentions
				.ToReadOnlyReactiveCollection()
				.AddTo(_CompositeDisposable);

			AddAcceptExtentionCommand = ExtentionText
				.Select(x => AppPolicy.CanAddAcceptExtention(x))
				.ToReactiveCommand()
				.AddTo(_CompositeDisposable);

			AddAcceptExtentionCommand
				.Select(x => ExtentionText.Value)
				.Subscribe(x => 
				{
					AppPolicy.AddAcceptExtention(x);

					ExtentionText.Value = "";
				})
				.AddTo(_CompositeDisposable);


			InputDeclaration = new AppOptionDeclarationViewModel(this, AppPolicy.InputOption);

			OutputOptionDeclarations = AppPolicy.OutputOptionDeclarations.ToReadOnlyReactiveCollection(
				x => new AppOptionDeclarationViewModel(this, x)
				)
				.AddTo(_CompositeDisposable);

			OptionDeclarations = AppPolicy.OptionDeclarations.ToReadOnlyReactiveCollection(
				x => new AppOptionDeclarationViewModel(this, x)
				)
				.AddTo(_CompositeDisposable);

			AppPathState = Observable.Merge(
					AppPolicyManager.Security.AuthorizedApplicationPathList.CollectionChangedAsObservable()
						.Where(x => AppPolicyManager.Security.IsAuthorized(AppPolicy)).ToUnit(),
					AppPolicy.ObserveProperty(x => x.ApplicationPath).ToUnit(),
					AppPolicy.ObserveProperty(x => x.ApplicationCheckSum).ToUnit()
					)
					.Select(x => 
					{
						if (String.IsNullOrEmpty(AppPolicy.ApplicationPath))
						{
							return ApplicationPathState.NotSelected;
						}
						else if (false == File.Exists(AppPolicy.ApplicationPath))
						{
							return ApplicationPathState.Missing;
						}
						else if (false == AppPolicyManager.Security.IsAuthorized(AppPolicy))
						{
							return ApplicationPathState.NotAuthorized;
						}
						else
						{
							return ApplicationPathState.Ready;
						}
					})
				.ToReactiveProperty();

			AppPathState.Subscribe()
				.AddTo(_CompositeDisposable);
		}


		private DelegateCommand _ChangeApplicationPathCommand;
		public DelegateCommand ChangeApplicationPathCommand
		{
			get
			{
				return _ChangeApplicationPathCommand
					?? (_ChangeApplicationPathCommand = new DelegateCommand(() =>
					{
						var dialog = new OpenFileDialog();

						dialog.Multiselect = false;
						dialog.Filter = "Application Files (.exe)|*.exe|All Files (*.*)|*.*";
						dialog.CheckFileExists = true;

						var result = dialog.ShowDialog();

						if (result != null && ((bool)result) == true)
						{



							// Note: 

							AppPolicyManager.Security.AuthorizeApplication(AppPolicy, dialog.FileName);

							EditPageVM.SaveCommand.Execute();



						}
					}));
			}
		}


		public ReactiveCommand AddAcceptExtentionCommand { get; private set; }


		private DelegateCommand<string> _RemoveAcceptExtentionCommand;
		public DelegateCommand<string> RemoveAcceptExtentionCommand
		{
			get
			{
				return _RemoveAcceptExtentionCommand
					?? (_RemoveAcceptExtentionCommand = new DelegateCommand<string>((extention) =>
					{
						AppPolicy.RemoveAcceptExtention(extention);
					}));
			}
		}



		
		private DelegateCommand _AddOptionDeclarationCommand;
		public DelegateCommand AddOptionDeclarationCommand
		{
			get
			{
				return _AddOptionDeclarationCommand
					?? (_AddOptionDeclarationCommand = new DelegateCommand(() =>
					{
						var decl = AppPolicy.AddOptionDeclaration("Option");

						// 編集ダイアログで開く
						OpenOptionDeclarationEditDialog(decl);
					}));
			}
		}


		private DelegateCommand _AddOutputOptionDeclarationCommand;
		public DelegateCommand AddOutputOptionDeclarationCommand
		{
			get
			{
				return _AddOutputOptionDeclarationCommand
					?? (_AddOutputOptionDeclarationCommand = new DelegateCommand(() =>
					{
						var decl = AppPolicy.AddOutputOptionDeclaration("Output Option");

						// 編集ダイアログで開く
						OpenOptionDeclarationEditDialog(decl);
					}));
			}
		}





		public async void OpenOptionDeclarationEditDialog<T>(T decl)
			where T : AppOptionDeclarationBase
		{
			var backupDecl = FileSerializeHelper.ToJson(decl);

			AppOptionDeclarationViewModel tempVM = new AppOptionDeclarationViewModel(this, decl);


			var view = new Views.DialogContent.OptionDeclarationEditDialogCcontent()
			{
				DataContext = tempVM
			};

			var result = await DialogHost.Show(view, "AppPolicyEditDialogHost");

			if (result == null || ((bool)result) == false)
			{
				// ロールバック
				var rollbackModel = FileSerializeHelper.FromJson<T>(backupDecl);

				decl.Rollback(rollbackModel);
			}

			tempVM.Dispose();
		}



		internal void RemoveDeclaration(AppOptionDeclarationBase declaration)
		{
			// TODO: AppOptionDeclarationの削除確認ダイアログの表示
			if (declaration is AppOptionDeclaration)
			{
				AppPolicy.RemoveOptionDeclaration(declaration as AppOptionDeclaration);
			}
			else if (declaration is AppOutputOptionDeclaration)
			{
				AppPolicy.RemoveOptionDeclaration(declaration as AppOutputOptionDeclaration);
			}
			else
			{
				throw new NotSupportedException("can not remove AppInputOptionDeclaration");
			}
		}

		public void Dispose()
		{
			_CompositeDisposable.Dispose();
		}

		
	}



	public enum ApplicationPathState
	{
		NotSelected,
		Missing,
		NotAuthorized,
		Ready,
	}



	


	
}
