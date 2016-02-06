﻿using Prism.Commands;
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
using Microsoft.Win32;

namespace Modules.AppPolicy.ViewModels
{
	public class AppPolicyListPageViewModel : PageViewModelBase, INavigationAware
	{

		public IRegionNavigationService NavigationService;



		public static ReadOnlyReactiveCollection<AppPolicyListItemViewModel> AppPolicies { get; private set; }



		public AppPolicyListPageViewModel(IRegionManager regionManager, IAppPolicyManager appPolicyManager)
			: base(regionManager, appPolicyManager)
		{
			AppPolicies = _AppPolicyManager.Policies
				.ToReadOnlyReactiveCollection(x => new AppPolicyListItemViewModel(this, x));
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

			
		}



		public void ShowAppPolicyEditPage(ApplicationPolicy appPolicy)
		{
			base.NavigationToAppPolicyEditPage(appPolicy.AppName);
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
						// ファイル選択ダイアログでexe等のアプリファイルのパスを選択してもらう
						var dialog = new OpenFileDialog();

						dialog.Multiselect = false;
						dialog.Title = "ReactiveFolder: Select application.";

						dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

						var result = dialog.ShowDialog();

						if (result.HasValue && result.Value)
						{
							var path = dialog.FileName;
							var name = dialog.SafeFileName;

							// パスが確認されたらApplicationPolicyを作成してReactiveFolderアプリ空間内に
							// アプリポリシーファイルを作成させる

							// TODO: 作成に失敗した場合の処理
							var newAppPolicy = new ApplicationPolicy(path);
							_AppPolicyManager.AddAppPolicy(newAppPolicy);


							// Edit画面へ
							ShowAppPolicyEditPage(newAppPolicy);
						}


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
						// TODO: アプリポリシーのインポート
						// アプリのパスを確認する
						// ProgramFiles等の強い権限が必要なファイルパスの場合は、自動チェックの対象外として
						// 必ずアプリパスの再指定を要求する


					}));
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
	}


	public class AppPolicyListItemViewModel : BindableBase
	{
		public AppPolicyListPageViewModel PageVM { get; private set; }
		public ApplicationPolicy AppPolicy { get; private set; }



		public string AppName { get; private set; }

		// TODO: 拡張子
		// TODO: アイコン画像

		public AppPolicyListItemViewModel(AppPolicyListPageViewModel pageVM, ApplicationPolicy appPolicy)
		{
			PageVM = pageVM;
			AppPolicy = appPolicy;

			AppName = AppPolicy.AppName;
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
	}
}