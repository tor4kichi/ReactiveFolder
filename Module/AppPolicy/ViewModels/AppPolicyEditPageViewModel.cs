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
						AppPolicy.AppOutputFormats.ObserveElementPropertyChanged().ToUnit()
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

		public ReactiveProperty<string> DefaultOptionText { get; private set; }

		public ReactiveProperty<string> ExtentionText { get; private set; }

		public ReactiveProperty<bool> IsInputFile { get; private set; }

		public ReactiveProperty<bool> IsOutputFile { get; private set; }

		// 入力可能なファイル拡張子（複数）
		public ReadOnlyReactiveCollection<string> AcceptExtentions { get; private set; }

		// 機能定義（複数）
		public ReadOnlyReactiveCollection<AppPolicyArgumentViewModel> AppArguments { get; private set; }



		public ApplicationPolicyViewModel(ApplicationPolicy appPolicy)
		{
			AppPolicy = appPolicy;

			_CompositeDisposable = new CompositeDisposable();

			ApplicationPath = AppPolicy.ObserveProperty(x => x.ApplicationPath)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			AppName = AppPolicy.ToReactivePropertyAsSynchronized(x => x.AppName)
				.AddTo(_CompositeDisposable);


			DefaultOptionText = AppPolicy
				.ToReactivePropertyAsSynchronized(x => x.DefaultOptionText)
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

			AppArguments = AppPolicy.AppOutputFormats.ToReadOnlyReactiveCollection(
				x => new AppPolicyArgumentViewModel(this, AppPolicy, x)
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



		private DelegateCommand _AddArgumentCommand;
		public DelegateCommand AddArgumentCommand
		{
			get
			{
				return _AddArgumentCommand
					?? (_AddArgumentCommand = new DelegateCommand(() =>
					{
						var newArg = AppPolicy.AddNewOutputFormat();

						var vm = new AppPolicyArgumentViewModel(this, AppPolicy, newArg);

						OpenArgumentEditDialog(vm);
					}));
			}
		}

		




		public async void OpenArgumentEditDialog(AppPolicyArgumentViewModel argumentVM)
		{
			var tempVM = new AppPolicyArgumentViewModel(this, AppPolicy, 
				new AppOutputFormat(-1)
				{
					Name = argumentVM.Argument.Name,
					OptionText = argumentVM.Argument.OptionText,
					Description = argumentVM.Argument.Description,
					OutputExtention = argumentVM.Argument.OutputExtention
				}
			);

			var view = new Views.AppPolicyArgumentEditDialog()
			{
				DataContext = tempVM
			};

			var result = await DialogHost.Show(view, "AppPolicyEditDialogHost");

			if (result != null && ((bool)result) == true)
			{
				// 変更を適用する
				var tempArg = tempVM.Argument;
				argumentVM.Argument.Name			= tempArg.Name;
				argumentVM.Argument.OptionText		= tempArg.OptionText;
				argumentVM.Argument.Description		= tempArg.Description;
				argumentVM.Argument.OutputExtention = tempArg.OutputExtention;
			}

			tempVM.Dispose();
		}


		public void RemoveArgument(AppPolicyArgumentViewModel argumentVM)
		{
			// TODO: AppArgumentの削除 確認ダイアログの表示
			AppPolicy.RemoveOutputFormat(argumentVM.Argument);
		}

		public void Dispose()
		{
			_CompositeDisposable.Dispose();
		}
	}













	public class AppPolicyArgumentViewModel : BindableBase, IDisposable
	{
		public ApplicationPolicyViewModel AppPolicyVM { get; private set; }
		public ApplicationPolicy AppPolicy { get; private set; }

		public AppOutputFormat Argument { get; private set; }


		public ReactiveProperty<string> ArgumentName { get; private set; }
		public ReactiveProperty<string> Description { get; private set; }
		public ReactiveProperty<string> OptionText { get; private set; }
		public ReactiveProperty<string> OutputExtention { get; private set; }

		public ReactiveProperty<string> FinalOptionText { get; private set; }

		public AppPolicyArgumentViewModel(ApplicationPolicyViewModel appPolicyVM, ApplicationPolicy appPolicy, AppOutputFormat argument)
		{
			this.AppPolicyVM = appPolicyVM;
			this.AppPolicy = appPolicy;
			this.Argument = argument;

			ArgumentName = this.Argument.ToReactivePropertyAsSynchronized(x => x.Name);
			Description = this.Argument.ToReactivePropertyAsSynchronized(x => x.Description);
			OptionText = this.Argument.ToReactivePropertyAsSynchronized(x => x.OptionText);
			OutputExtention = this.Argument.ToReactivePropertyAsSynchronized(x => x.OutputExtention);


			FinalOptionText = Observable.Merge(
				AppPolicyVM.InputPathType.ToUnit(),
				AppPolicyVM.OutputPathType.ToUnit(),
				AppPolicyVM.DefaultOptionText.ToUnit(),
				OptionText.ToUnit(),
				OutputExtention.ToUnit()
				)
				.Select(_ =>
				{
					var ext = AppPolicy.AcceptExtentions.FirstOrDefault() ?? ".ext";
					var dummyInput = "dummyFolder/filename" + ext;
					var dummyFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
					return AppPolicy.MakeArgumentsText(dummyInput, new DirectoryInfo(dummyFolder), Argument);
				})
				.ToReactiveProperty();
		}

		private DelegateCommand _EditArgumentCommand;
		public DelegateCommand EditArgumentCommand
		{
			get
			{
				return _EditArgumentCommand
					?? (_EditArgumentCommand = new DelegateCommand(() =>
					{
						AppPolicyVM.OpenArgumentEditDialog(this);
					}));
			}
		}

		private DelegateCommand _RemoveArgumentCommand;
		public DelegateCommand RemoveArgumentCommand
		{
			get
			{
				return _RemoveArgumentCommand
					?? (_RemoveArgumentCommand = new DelegateCommand(() =>
					{
						AppPolicyVM.RemoveArgument(this);
					}));
			}
		}


		public void Dispose()
		{
			ArgumentName.Dispose();
			Description.Dispose();
			OptionText.Dispose();
			OutputExtention.Dispose();

			FinalOptionText.Dispose();
		}
	}
}
