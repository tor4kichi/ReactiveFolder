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

		public ReactiveProperty<bool> IsInputFile { get; private set; }

		public ReactiveProperty<bool> IsOutputFile { get; private set; }



		public ReactiveProperty<string> ExtentionText { get; private set; }

		// 入力可能なファイル拡張子（複数）
		public ReadOnlyReactiveCollection<string> AcceptExtentions { get; private set; }

		// 機能定義（複数）
		public ReadOnlyReactiveCollection<AppPolicyOutputFormatViewModel> AppOutputFormats { get; private set; }


		// オプションのプロトタイプ宣言
		public ReadOnlyReactiveCollection<AppOptionDeclarationViewModel> OptionDeclarations { get; private set; }


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




			AppOutputFormats = AppPolicy.AppOutputFormats.ToReadOnlyReactiveCollection(
				x => new AppPolicyOutputFormatViewModel(this, AppPolicy, x)
				)
				.AddTo(_CompositeDisposable);



			OptionDeclarations = AppPolicy.OptionDeclarations.ToReadOnlyReactiveCollection(
				x => new AppOptionDeclarationViewModel(this, x)
				)
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


		private DelegateCommand _AddOptionDeclarationCommand;
		public DelegateCommand AddOptionDeclarationCommand
		{
			get
			{
				return _AddOptionDeclarationCommand
					?? (_AddOptionDeclarationCommand = new DelegateCommand(() =>
					{
						var decl = AppPolicy.AddNewOptionDeclaration();

						// 編集ダイアログで開く
						OpenOptionDeclarationEditDialog(decl);
					}));
			}
		}


		// TODO: OpenOutputFormatEditDialog: VM じゃなくてModelを直接引数で受け取るようにする
		public async void OpenOutputFormatEditDialog(AppPolicyOutputFormatViewModel outputFormatVM)
		{
			var tempOutputFormatModel = new AppOutputFormat(-1)
			{
				Name = outputFormatVM.OutputFormat.Name,
				OutputExtention = outputFormatVM.OutputFormat.OutputExtention
			};

			foreach (var option in outputFormatVM.OutputFormat.OptionValueSets)
			{
				var decl = AppPolicy.FindOptionDeclaration(option.OptionId);
				tempOutputFormatModel.AddOption(decl);
				tempOutputFormatModel.UpdateOption(decl, option.Values.ToArray());
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

				foreach (var option in tempArg.OptionValueSets)
				{
					var decl = AppPolicy.FindOptionDeclaration(option.OptionId);
					outputFormatVM.OutputFormat.UpdateOption(decl, option.Values.ToArray());
				}
			}

			tempVM.Dispose();
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

				decl.Name = rollbackModel.Name;
				decl.Order = rollbackModel.Order;
				decl.OptionTextPattern = rollbackModel.OptionTextPattern;

				if (decl is AppOptionDeclaration)
				{
					var userDecl = decl as AppOptionDeclaration;
					var removePropties = userDecl.UserProperties.ToArray();
					foreach (var remProp in removePropties)
					{
						userDecl.RemoveProperty(remProp);
					}

					foreach (var addProp in rollbackModel.UserProperties)
					{
						userDecl.AddProperty(addProp);
					}
				}
			}

			tempVM.Dispose();
		}

		internal void RemoveOutputFormat(AppPolicyOutputFormatViewModel outputFormatVM)
		{
			// TODO: AppOutputFormatの削除 確認ダイアログの表示
			AppPolicy.RemoveOutputFormat(outputFormatVM.OutputFormat);
		}


		internal void RemoveDeclaration(AppOptionDeclarationBase declaration)
		{
			// TODO: AppOptionDeclarationの削除確認ダイアログの表示
			AppPolicy.RemoveOptionDeclaration(declaration);
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

		public ReadOnlyReactiveCollection<OptionValueSetViewModel> Options { get; private set; }
		 

		public AppPolicyOutputFormatViewModel(ApplicationPolicyViewModel appPolicyVM, ApplicationPolicy appPolicy, AppOutputFormat outputFormat)
		{
			this.AppPolicyVM = appPolicyVM;
			this.AppPolicy = appPolicy;
			this.OutputFormat = outputFormat;
			

			OutputFormatName = this.OutputFormat.ToReactivePropertyAsSynchronized(x => x.Name);
			OutputExtention = this.OutputFormat.ToReactivePropertyAsSynchronized(x => x.OutputExtention);


			Options = this.OutputFormat.OptionValueSets.ToReadOnlyReactiveCollection(
				x => new OptionValueSetViewModel(this, AppPolicy.FindOptionDeclaration(x.OptionId), x)
				);


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

		internal void AddOption(AppOptionDeclarationBase appOptionDecl)
		{
			OutputFormat.AddOption(appOptionDecl);
		}
	}


	public class OptionDeclarationViewModel : BindableBase
	{
		public AppPolicyOutputFormatViewModel OutputFormatVM { get; private set; }
		public AppOptionDeclarationBase AppOptionDecl { get; private set; }


		public string OptionName { get; private set; }


		public OptionDeclarationViewModel(AppPolicyOutputFormatViewModel outputFormatVM, AppOptionDeclarationBase decl)
		{
			OutputFormatVM = outputFormatVM;
			AppOptionDecl = decl;


			OptionName = AppOptionDecl.Name;
		}



		private DelegateCommand _AddOptionCommand;
		public DelegateCommand AddOptionCommand
		{
			get
			{
				return _AddOptionCommand
					?? (_AddOptionCommand = new DelegateCommand(() =>
					{
						OutputFormatVM.AddOption(AppOptionDecl);
					}));
			}
		}
	}

	public class OptionValueSetViewModel : BindableBase, IDisposable
	{
		public AppPolicyOutputFormatViewModel OutputFormatVM { get; private set; }
		public AppOptionDeclarationBase AppOptionDecl { get; private set; }
		public AppOptionValueSet ValueSet { get; private set; }


		public string OptionName { get; private set; }


		public List<OptionValueViewModel> Properties { get; private set; }

		public OptionValueSetViewModel(AppPolicyOutputFormatViewModel outputFormatVM, AppOptionDeclarationBase decl, AppOptionValueSet valueSet)
		{
			OutputFormatVM = outputFormatVM;
			AppOptionDecl = decl;
			ValueSet = valueSet;

			OptionName = AppOptionDecl.Name;

			Properties = decl.UserProperties.Join(ValueSet.Values,
				(x) => x.ValiableName,
				(y) => y.ValiableName,
				(x, y) => ToOptionPropertyVM(x, y)
				)
				.ToList();
		}


		private OptionValueViewModel ToOptionPropertyVM(AppOptionProperty optionProp, AppOptionValue val)
		{
			if (optionProp is IOPathAppOptionProperty)
			{
				return new IOPathOptionValueViewModel(optionProp as IOPathAppOptionProperty, val);
			} 
			else if (optionProp is StringListOptionProperty)
			{
				return new StringListOptionValueViewModel(optionProp as StringListOptionProperty, val);
			}

			// Note: 優先度： RangeNumber > Number 
			else if (optionProp is RangeNumberAppOptionProperty)
			{
				return new RangeNumberOptionValueViewModel(optionProp as RangeNumberAppOptionProperty, val);
			}
			else if (optionProp is NumberAppOptionProperty)
			{
				return new NumberOptionValueViewModel(optionProp as NumberAppOptionProperty, val);
			}

			throw new Exception();
		}

		public void Dispose()
		{
			foreach (var prop in this.Properties)
			{
				prop.Dispose();
			}
		}
	}



	
}
