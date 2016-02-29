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
using Microsoft.Win32;
using ReactiveFolder.Models.Util;
using ReactiveFolderStyles.DialogContent;
using MaterialDesignThemes.Wpf;
using ReactiveFolder.Models.Actions;

namespace Modules.AppPolicy.ViewModels
{
	public class AppPolicyManagePageViewModel : PageViewModelBase, INavigationAware
	{
		public IFolderReactionMonitorModel Monitor { get; private set; }
		public IRegionNavigationService NavigationService { get; private set; }



		public static ReadOnlyReactiveCollection<AppPolicyListItemViewModel> AppPolicies { get; private set; }


		public ReactiveProperty<AppPolicyEditControlViewModel> AppPolicyEditVM { get; private set; }


		public AppPolicyManagePageViewModel(IRegionManager regionManager, IAppPolicyManager appPolicyManager, IFolderReactionMonitorModel monitor)
			: base(regionManager, appPolicyManager)
		{
			Monitor = monitor;
			AppPolicies = _AppPolicyManager.Policies
				.ToReadOnlyReactiveCollection(x => new AppPolicyListItemViewModel(this, x));

			AppPolicyEditVM = new ReactiveProperty<AppPolicyEditControlViewModel>();
		}


		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
			// nothing to do.
			if (AppPolicyEditVM.Value != null)
			{
				ExitEdit();
			}
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			NavigationService = navigationContext.NavigationService;

		}



		public void ShowAppPolicyEditPage(ApplicationPolicy appPolicy)
		{
			if (AppPolicyEditVM.Value?.AppPolicy != appPolicy)
			{
				if (AppPolicyEditVM.Value != null)
				{
					ExitEdit();
				}

				SetupEdit(appPolicy);
			}
		}


		public void SetupEdit(ApplicationPolicy appPolicy)
		{
			try
			{
				// 関連するリアクションの動作を止める
				var relativeReactions = Monitor.RootFolder.Reactions.Where(x => x.Actions.Any(y => (y as AppLaunchReactiveAction)?.AppPolicy == appPolicy));

				foreach (var reaction in relativeReactions)
				{
					Monitor.StopMonitoring(reaction);
				}

				// VMを設定
				AppPolicyEditVM.Value = new AppPolicyEditControlViewModel(appPolicy, _RegionManager, _AppPolicyManager);


				// リスト表示を更新
				var selectedListItem = AppPolicies.SingleOrDefault(x => x.AppPolicy == appPolicy);
				if (selectedListItem != null)
				{
					selectedListItem.IsSelected = true;
				}
			}
			catch(Exception e)
			{
				ExitEdit();
			}




		}

		public void ExitEdit()
		{
			var appPolicy = AppPolicyEditVM.Value?.AppPolicy;

			AppPolicyEditVM.Value?.Save();


			// リスト表示を更新
			foreach (var listItem in AppPolicies)
			{
				listItem.IsSelected = false;
			}

			// VMを解放
			AppPolicyEditVM.Value?.Dispose();
			AppPolicyEditVM.Value = null;

			// 関連するリアクションの動作を再開させる
			var relativeReactions = Monitor.RootFolder.Reactions.Where(x => x.Actions.Any(y => (y as AppLaunchReactiveAction)?.AppPolicy == appPolicy));

			foreach (var reaction in relativeReactions)
			{
				Monitor.StartMonitoring(reaction);
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



		private DelegateCommand _AddAppPolicyCommand;
		public DelegateCommand AddAppPolicyCommand
		{
			get
			{
				return _AddAppPolicyCommand
					?? (_AddAppPolicyCommand = new DelegateCommand(() =>
					{
						var newAppPolicy = new ApplicationPolicy();

						Task.Run(() =>
						{
							_AppPolicyManager.AddAppPolicy(newAppPolicy);
						})
						.ContinueWith(x => 
						{
							ShowAppPolicyEditPage(newAppPolicy);
						});
					}));
			}
		}


		private DelegateCommand _ImportAppPolicyCommand;
		public DelegateCommand ImportAppPolicyCommand
		{
			get
			{
				return _ImportAppPolicyCommand
					?? (_ImportAppPolicyCommand = new DelegateCommand(() =>
					{
						// アプリのパスを確認する
						// ProgramFiles等の強い権限が必要なファイルパスの場合は、自動チェックの対象外として
						// 必ずアプリパスの再指定を要求する

						var dialog = new OpenFileDialog();

						dialog.Title = "ReactiveFolder - Import Application Policy";
						dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

						dialog.Filter = $"App Policy|*{_AppPolicyManager.PolicyFileExtention}|Json|*.json|All|*.*";
						dialog.DefaultExt = _AppPolicyManager.PolicyFileExtention;
						dialog.Multiselect = true;

						var result = dialog.ShowDialog();


						if (result != null && ((bool)result) == true)
						{
							foreach(var destFilePath in dialog.FileNames)
							{
								ImportApplicationPolicy(destFilePath);
							}
						}
					}));
			}

		}


		private void ImportApplicationPolicy(string path)
		{
			var appPolicy = FileSerializeHelper.LoadAsync<ApplicationPolicy>(path);

			if (_AppPolicyManager.HasAppPolicy(appPolicy))
			{
				// TODO: インポートしたファイルをGuidを強制的に書き換えて別ファイルとして取り込む
				// ApplicationPolicyにエクスポート実行者によるバージョン管理機能があればベター？

			}
			else
			{
				_AppPolicyManager.AddAppPolicy(appPolicy);
			}

		}


		private DelegateCommand _ShowHelpCommand;
		public DelegateCommand ShowHelpCommand
		{
			get
			{
				return _ShowHelpCommand
					?? (_ShowHelpCommand = new DelegateCommand(() =>
					{
						// TODO: アプリポリシーって何？から使い方まで
						// 図で一発で説明できるとチョべりぐー


					}));
			}
		}


		internal async void DeleteAppPolicy(ApplicationPolicy appPolicy)
		{
			var result = await ShowAppPolicyDeleteConfirmDialog();

			if (result != null && ((bool)result) == true)
			{
				_AppPolicyManager.RemoveAppPolicy(appPolicy);


				if (AppPolicyEditVM.Value?.AppPolicy == appPolicy)
				{
					AppPolicyEditVM.Value = null;
				}
			}

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


	public class AppPolicyListItemViewModel : BindableBase
	{
		public AppPolicyManagePageViewModel PageVM { get; private set; }
		public ApplicationPolicy AppPolicy { get; private set; }



		public ReactiveProperty<string> AppName { get; private set; }

		private bool _IsSelected;
		public bool IsSelected
		{
			get
			{
				return _IsSelected;
			}
			set
			{
				SetProperty(ref _IsSelected, value);
			}
		}
		

		// TODO: アイコン画像

		public AppPolicyListItemViewModel(AppPolicyManagePageViewModel pageVM, ApplicationPolicy appPolicy)
		{
			PageVM = pageVM;
			AppPolicy = appPolicy;

			AppName = AppPolicy.ObserveProperty(x => x.AppName)
				.ToReactiveProperty();

			IsSelected = false;
		}



		private DelegateCommand _SelectAppCommand;
		public DelegateCommand SelectAppCommand
		{
			get
			{
				return _SelectAppCommand
					?? (_SelectAppCommand = new DelegateCommand(() =>
					{
						PageVM.ShowAppPolicyEditPage(AppPolicy);
					}));
			}
		}

		private DelegateCommand _DeleteCommand;
		public DelegateCommand DeleteCommand
		{
			get
			{
				return _DeleteCommand
					?? (_DeleteCommand = new DelegateCommand(() =>
					{
						PageVM.DeleteAppPolicy(AppPolicy);
					}));
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
						var appPolicy = this.AppPolicy;
						var appPolicyManager = PageVM._AppPolicyManager;
						// 出力先のファイル名を取得
						var dialog = new SaveFileDialog();

						dialog.Title = "ReactiveFolder - select export application policy file";
						dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

						dialog.AddExtension = true;
						dialog.FileName = appPolicy.AppName;
						dialog.Filter = $"App Policy|*{appPolicyManager.PolicyFileExtention}|Json|*.json|All|*.*";
						dialog.DefaultExt = appPolicyManager.PolicyFileExtention;


						var result = dialog.ShowDialog();


						if (result != null && ((bool)result) == true)
						{
							var destFilePath = dialog.FileName;
							var destFileInfo = new FileInfo(destFilePath);

							try
							{
								if (destFileInfo.Exists)
								{
									destFileInfo.Delete();
								}

								FileSerializeHelper.Save(destFileInfo, AppPolicy);
							}
							catch
							{
								System.Diagnostics.Debug.WriteLine("failed export Application Policy.");
								System.Diagnostics.Debug.WriteLine("  to :" + destFilePath);
							}
						}



						// 
					}));
			}
		}
	}
}
