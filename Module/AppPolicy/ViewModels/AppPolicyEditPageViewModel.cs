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
using ReactiveFolderStyles.ViewModels;
using ReactiveFolderStyles.Models;

namespace Modules.AppPolicy.ViewModels
{
	public class AppPolicyEditPageViewModel : PageViewModelBase, IDisposable
	{
		public IAppPolicyManager AppPolicyManager { get; private set; }

		public ReactiveProperty<ApplicationPolicyViewModel> AppPolicyVM { get; private set; }


		public string RollbackData { get; private set; }


		public ReactiveProperty<bool> IsNeedSave { get; private set; }

		private IDisposable CanSaveSubscriber;

		public AppPolicyEditPageViewModel(PageManager pageManager, IAppPolicyManager appPolicyManager)
			: base(pageManager)
		{
			AppPolicyManager = appPolicyManager;
			AppPolicyVM = new ReactiveProperty<ApplicationPolicyViewModel>();
			IsNeedSave = new ReactiveProperty<bool>(false);

			SaveCommand = IsNeedSave.ToReactiveCommand(false);

			SaveCommand.Subscribe(_ => Save());

			IsNeedSave.Value = false;

			/*
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

			*/
		}


		public void Dispose()
		{
			AppPolicyVM?.Dispose();
			IsNeedSave?.Dispose();
			CanSaveSubscriber?.Dispose();
			SaveCommand?.Dispose();
		}


		public ApplicationPolicy AppPolicy
		{
			get
			{
				return AppPolicyVM.Value?.AppPolicy;
			}
		}


		public ReactiveCommand SaveCommand { get; private set; }

		internal void Save()
		{
			IsNeedSave.Value = false;

			try
			{
				AppPolicyManager.SavePolicyFile(this.AppPolicy);	
			}
			catch
			{
				IsNeedSave.Value = true;
			}
		}

		public override void OnNavigatedTo(NavigationContext navigationContext)
		{
			PageManager.IsOpenSubContent = false;

			if (navigationContext.Parameters.Count() > 0)
			{
				if (navigationContext.Parameters.Any(x => x.Key == "guid"))
				{
					try
					{
						var appPolicyGuid = (Guid)navigationContext.Parameters["guid"];

						AppPolicyVM.Value = new ApplicationPolicyViewModel(this, AppPolicyManager, AppPolicyManager.FromAppGuid(appPolicyGuid));

						PageManager.IsOpenSubContent = true;
					}
					catch
					{
						Console.WriteLine("FolderReactionManagePage: パラメータが不正です。存在するReactionのGuidを指定してください。");
					}
				}
			}
		}

		public override void OnNavigatedFrom(NavigationContext navigationContext)
		{
			
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
					AppPolicyManager.Security.AppAuthoricationList.CollectionChangedAsObservable()
						.Where(x => AppPolicyManager.Security.IsAuthorized(AppPolicy.ApplicationPath)).ToUnit(),
					AppPolicy.ObserveProperty(x => x.ApplicationPath).ToUnit()
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
						else if (false == AppPolicyManager.Security.IsAuthorized(AppPolicy.ApplicationPath))
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
							AppPolicyManager.Security.AuthorizeApplication(dialog.FileName);

							AppPolicy.ApplicationPath = dialog.FileName;
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
