using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;
using System.IO;
using ReactiveFolder.Model.AppPolicy;
using MaterialDesignThemes.Wpf;
using System.Reactive.Linq;
using ReactiveFolder.Model.Util;
using Microsoft.Win32;

namespace Modules.AppPolicy.ViewModels
{
	public class AppPolicyEditPageViewModel : PageViewModelBase, INavigationAware
	{
		public IRegionNavigationService NavigationService;


		public ReactiveProperty<ApplicationPolicyViewModel> AppPolicyVM { get; private set; }


		public AppPolicyEditPageViewModel(IRegionManager regionManager, IAppPolicyManager appPolicyManager)
			: base(regionManager, appPolicyManager)
		{
			AppPolicyVM = new ReactiveProperty<ApplicationPolicyViewModel>();
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
				var policy = base.ApplicationPolicyFromNavigationParameters(navigationContext.Parameters);

				AppPolicyVM.Value = new ApplicationPolicyViewModel(policy);
			}
			catch
			{
				NavigationService.Journal.GoBack();
			}
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
							_RegionManager.RequestNavigate("MainRegion", "FolderListPage");
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
						_AppPolicyManager.SavePolicyFile(this.AppPolicyVM.Value.AppPolicy);
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
						var confirmDialogContent = new Views.DeleteConfirmDialog();


						var result = await DialogHost.Show(confirmDialogContent, "DeleteConfirmDialog");

						if (result != null && ((bool)result) == true)
						{
							_AppPolicyManager.RemoveAppPolicy(this.AppPolicyVM.Value.AppPolicy);

							NavigationService.Journal.GoBack();
						}
					}));
			}
		}
	}


	public class ApplicationPolicyViewModel : BindableBase
	{
		public static FolderItemType[] FolderItemTypes = (FolderItemType[]) Enum.GetValues(typeof(FolderItemType));


		public ApplicationPolicy AppPolicy { get; private set; }

		public ReactiveProperty<string> ApplicationPath { get; private set; }

		public ReactiveProperty<string> AppName { get; private set; }

		
		public ReactiveProperty<FolderItemType> InputPathType { get; private set; }

		public ReactiveProperty<FolderItemType> OutputPathType { get; private set; }

		public ReactiveProperty<string> DefaultOptionText { get; private set; }

		public ReactiveProperty<string> ExtentionText { get; private set; }

		// 入力可能なファイル拡張子（複数）
		public ReadOnlyReactiveCollection<string> AcceptExtentions { get; private set; }

		// 機能定義（複数）
		public ReadOnlyReactiveCollection<AppPolicyArgumentViewModel> AppArguments { get; private set; }



		public ApplicationPolicyViewModel(ApplicationPolicy appPolicy)
		{
			AppPolicy = appPolicy;



			ApplicationPath = AppPolicy.ObserveProperty(x => x.ApplicationPath)
				.ToReactiveProperty();

			AppName = AppPolicy.ToReactivePropertyAsSynchronized(x => x.AppName);


			DefaultOptionText = AppPolicy
				.ToReactivePropertyAsSynchronized(x => x.DefaultOptionText);

			InputPathType = AppPolicy.ToReactivePropertyAsSynchronized(x => x.InputPathType);

			OutputPathType = AppPolicy.ToReactivePropertyAsSynchronized(x => x.OutputPathType);


			AcceptExtentions = appPolicy.AcceptExtentions
				.ToReadOnlyReactiveCollection();

			AppArguments = AppPolicy.AppParams.ToReadOnlyReactiveCollection(
				x => new AppPolicyArgumentViewModel(this, AppPolicy, x)
				);

			ExtentionText = new ReactiveProperty<string>("");


			AddAcceptExtentionCommand = ExtentionText
				.Select(x => AppPolicy.CanAddAcceptExtention(x))
				.ToReactiveCommand();


			AddAcceptExtentionCommand
				.Select(x => ExtentionText.Value)
				.Subscribe(x => 
			{
				AppPolicy.AddAcceptExtention(x);

				ExtentionText.Value = "";
			});
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
						// TODO: 

						var newArg = AppPolicy.AddNewArgument();


						//						var vm = AppArguments.Single(x => x.Argument == newArg);
						var vm = new AppPolicyArgumentViewModel(this, AppPolicy, newArg);

						OpenArgumentEditDialog(vm);
					}));
			}
		}

		




		public async void OpenArgumentEditDialog(AppPolicyArgumentViewModel argumentVM)
		{
			var tempVM = new AppPolicyArgumentViewModel(this, AppPolicy, 
				new AppArgument(-1)
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

			var result = await DialogHost.Show(view, "ArgumentEditDialog");

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
			// TODO: RemoveArgument show confirm dialog.
			AppPolicy.RemoveArgument(argumentVM.Argument);
		}


	}


	public class AppPolicyArgumentViewModel : BindableBase, IDisposable
	{
		public ApplicationPolicyViewModel AppPolicyVM { get; private set; }
		public ApplicationPolicy AppPolicy { get; private set; }

		public AppArgument Argument { get; private set; }


		public ReactiveProperty<string> ArgumentName { get; private set; }
		public ReactiveProperty<string> Description { get; private set; }
		public ReactiveProperty<string> OptionText { get; private set; }
		public ReactiveProperty<string> OutputExtention { get; private set; }

		public ReactiveProperty<string> FinalOptionText { get; private set; }

		public AppPolicyArgumentViewModel(ApplicationPolicyViewModel appPolicyVM, ApplicationPolicy appPolicy, AppArgument argument)
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
