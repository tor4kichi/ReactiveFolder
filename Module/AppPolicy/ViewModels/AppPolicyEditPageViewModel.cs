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
				AppPolicyVM.Value = new ApplicationPolicyViewModel(policy);

				BackupAppPolicyModel();

				IsNeedSave.Value = false;

				CanSaveSubscriber = Observable.Merge(
						AppPolicy.PropertyChangedAsObservable().ToUnit(),
						AppPolicy.AcceptExtentions.CollectionChangedAsObservable().ToUnit(),
						AppPolicy.AppOutputFormats.CollectionChangedAsObservable().ToUnit(),
						AppPolicy.AppOutputFormats.ObserveElementPropertyChanged().ToUnit(),
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

		public ApplicationPolicy AppPolicy { get; private set; }

		public ReactiveProperty<string> ApplicationPath { get; private set; }

		public ReactiveProperty<string> AppName { get; private set; }

		
		public ReactiveProperty<FolderItemType> InputPathType { get; private set; }

		public ReactiveProperty<FolderItemType> OutputPathType { get; private set; }

		public ReactiveProperty<string> ExtentionText { get; private set; }

		public ReactiveProperty<bool> IsInputFile { get; private set; }

		public ReactiveProperty<bool> IsOutputFile { get; private set; }

		// 入力可能なファイル拡張子（複数）
		public ReadOnlyReactiveCollection<string> AcceptExtentions { get; private set; }

		// 機能定義（複数）
		public ReadOnlyReactiveCollection<AppPolicyOutputFormatViewModel> AppOutputFormats { get; private set; }



		public ApplicationPolicyViewModel(ApplicationPolicy appPolicy)
		{
			AppPolicy = appPolicy;

			_CompositeDisposable = new CompositeDisposable();

			ApplicationPath = AppPolicy.ObserveProperty(x => x.ApplicationPath)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			AppName = AppPolicy.ToReactivePropertyAsSynchronized(x => x.AppName)
				.AddTo(_CompositeDisposable);


			InputPathType = AppPolicy.ToReactivePropertyAsSynchronized(x => x.InputPathType)
				.AddTo(_CompositeDisposable);

			OutputPathType = AppPolicy.ToReactivePropertyAsSynchronized(x => x.OutputPathType)
				.AddTo(_CompositeDisposable);

			IsInputFile = InputPathType.Select(x => x == FolderItemType.File)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			IsOutputFile = OutputPathType.Select(x => x == FolderItemType.File)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			AcceptExtentions = appPolicy.AcceptExtentions
				.ToReadOnlyReactiveCollection()
				.AddTo(_CompositeDisposable);

			AppOutputFormats = AppPolicy.AppOutputFormats.ToReadOnlyReactiveCollection(
				x => new AppPolicyOutputFormatViewModel(this, AppPolicy, x)
				)
				.AddTo(_CompositeDisposable);

			ExtentionText = new ReactiveProperty<string>("")
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
							AppPolicy.ApplicationPath = dialog.FileName;
						}
					}));
			}
		}


		public ReactiveCommand AddAcceptExtentionCommand { get; private set; }
/*
		private DelegateCommand _AddAcceptExtentionCommand;
		public DelegateCommand AddAcceptExtentionCommand
		{
			get
			{
				return _AddAcceptExtentionCommand
					?? (_AddAcceptExtentionCommand = new DelegateCommand(() =>
					{
						AppPolicy.AddAcceptExtention(ExtentionText.Value);

						ExtentionText.Value = "";
					},
					
					() => AppPolicy.CanAddAcceptExtention(ExtentionText.Value)
					));
			}
		}
*/

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



		private DelegateCommand _AddOutputFormatCommand;
		public DelegateCommand AddOutputFormatCommand
		{
			get
			{
				return _AddOutputFormatCommand
					?? (_AddOutputFormatCommand = new DelegateCommand(() =>
					{
						var newOutputFormat = AppPolicy.AddNewOutputFormat();

						var vm = new AppPolicyOutputFormatViewModel(this, AppPolicy, newOutputFormat);

						OpenOutputFormatEditDialog(vm);
					}));
			}
		}

		




		public async void OpenOutputFormatEditDialog(AppPolicyOutputFormatViewModel outputFormatVM)
		{
			var tempOutputFormatModel = new AppOutputFormat(-1)
			{
				Name = outputFormatVM.OutputFormat.Name,
				OutputExtention = outputFormatVM.OutputFormat.OutputExtention
			};

			foreach (var option in outputFormatVM.OutputFormat.Options)
			{
				var decl = AppPolicy.FindOptionDeclaration(option.OptionId);
				tempOutputFormatModel.AddOrUpdateOption(decl, option.Values.ToArray());
			}

			var tempVM = new AppPolicyOutputFormatViewModel(this, AppPolicy, tempOutputFormatModel);





			var view = new Views.AppPolicyOutputFormatEditDialog()
			{
				DataContext = tempVM
			};

			var result = await DialogHost.Show(view, "AppPolicyEditDialogHost");

			if (result != null && ((bool)result) == true)
			{
				// 変更を適用する
				var tempArg = tempVM.OutputFormat;
				outputFormatVM.OutputFormat.Name			= tempArg.Name;
				outputFormatVM.OutputFormat.OutputExtention = tempArg.OutputExtention;

				foreach (var option in tempArg.Options)
				{
					var decl = AppPolicy.FindOptionDeclaration(option.OptionId);
					outputFormatVM.OutputFormat.AddOrUpdateOption(decl, option.Values.ToArray());
				}
			}

			tempVM.Dispose();
		}


		public void RemoveOutputFormat(AppPolicyOutputFormatViewModel outputFormatVM)
		{
			// TODO: AppOutputFormatの削除 確認ダイアログの表示
			AppPolicy.RemoveOutputFormat(outputFormatVM.OutputFormat);
		}

		public void Dispose()
		{
			_CompositeDisposable.Dispose();
		}
	}











	// TODO: AppPolicyOutputFormatViewModelをAppOptionDeclarationに対応させる

	public class AppPolicyOutputFormatViewModel : BindableBase, IDisposable
	{
		public ApplicationPolicyViewModel AppPolicyVM { get; private set; }
		public ApplicationPolicy AppPolicy { get; private set; }

		public AppOutputFormat OutputFormat { get; private set; }


		public ReactiveProperty<string> OutputFormatName { get; private set; }
		public ReactiveProperty<string> OutputExtention { get; private set; }

		public ReactiveProperty<string> FinalOptionText { get; private set; }

		public AppPolicyOutputFormatViewModel(ApplicationPolicyViewModel appPolicyVM, ApplicationPolicy appPolicy, AppOutputFormat outputFormat)
		{
			this.AppPolicyVM = appPolicyVM;
			this.AppPolicy = appPolicy;
			this.OutputFormat = outputFormat;

			OutputFormatName = this.OutputFormat.ToReactivePropertyAsSynchronized(x => x.Name);
			OutputExtention = this.OutputFormat.ToReactivePropertyAsSynchronized(x => x.OutputExtention);


			FinalOptionText = Observable.Merge(
				AppPolicyVM.InputPathType.ToUnit(),
				AppPolicyVM.OutputPathType.ToUnit(),
				OutputExtention.ToUnit()
				)
				.Select(_ =>
				{
					var ext = AppPolicy.AcceptExtentions.FirstOrDefault() ?? ".ext";
					var dummyInput = "dummyFolder/filename" + ext;
					var dummyFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
					return AppPolicy.MakeCommandLineOptionText(dummyInput, new DirectoryInfo(dummyFolder), OutputFormat);
				})
				.ToReactiveProperty();
		}

		private DelegateCommand _EditOutputFormatCommand;
		public DelegateCommand EditOutputFormatCommand
		{
			get
			{
				return _EditOutputFormatCommand
					?? (_EditOutputFormatCommand = new DelegateCommand(() =>
					{
						AppPolicyVM.OpenOutputFormatEditDialog(this);
					}));
			}
		}

		private DelegateCommand _RemoveOutputFormatCommand;
		public DelegateCommand RemoveOutputFormatCommand
		{
			get
			{
				return _RemoveOutputFormatCommand
					?? (_RemoveOutputFormatCommand = new DelegateCommand(() =>
					{
						AppPolicyVM.RemoveOutputFormat(this);
					}));
			}
		}


		public void Dispose()
		{
			OutputFormatName.Dispose();
			OutputExtention.Dispose();

			FinalOptionText.Dispose();
		}
	}
}
