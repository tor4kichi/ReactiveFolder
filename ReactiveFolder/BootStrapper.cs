using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.Prism;
using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Unity;
using ReactiveFolder.Views;
using ReactiveFolder.Model;
using Prism.Modularity;

using ReactiveFolder.Properties;
using System.IO;
using ReactiveFolder.Model.Actions;
using ReactiveFolder.Model.AppPolicy;
using Microsoft.Practices.Prism.Regions;
using Prism.Events;

namespace ReactiveFolder
{
	class Bootstrapper : UnityBootstrapper
	{
		public const string MonitorSettingsFolderName = "ReactiveFolder";
		public static readonly string MonitorSettingsSaveFolderPath =
			new DirectoryInfo(
			Path.Combine(
					//				System.Windows.Forms.Application.LocalUserAppDataPath,
					Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					"ReactiveFolder"
				)
			).FullName;

		public const string ActionAppPolicyFolderName = "app_policy";
		public static readonly string ActionAppPolicyFolderPath =
			new DirectoryInfo(
			Path.Combine(
					System.Windows.Forms.Application.LocalUserAppDataPath,
					//Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					"app_policy"
				)
			).FullName;

		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}


		private FolderReactionMonitorModel InitializeMonitorModel()
		{
			var monitorSaveFolderPath = Properties.Settings.Default.MonitorDataSaveFolderPath;
			if (false == Directory.Exists(monitorSaveFolderPath))
			{
				monitorSaveFolderPath = MonitorSettingsSaveFolderPath;
				Properties.Settings.Default.MonitorDataSaveFolderPath = MonitorSettingsSaveFolderPath;
				Properties.Settings.Default.Save();
			}

			var model = FolderReactionMonitorModel.LoadOrCreate(
				new DirectoryInfo(MonitorSettingsSaveFolderPath)
				);

			return model;
		}


		/// <summary>
		/// 外部アプリの使用ポリシーのファイルを読み込んでAppPolicyFactoryを初期化する
		/// </summary>
		private IAppPolicyManager InitializeAppLaunchAction()
		{
			// Note: 使用ポリシーはアプリローカル空間に保存する
			var policySaveFolderPath = ActionAppPolicyFolderPath;

			var policySaveFolderInfo = new DirectoryInfo(policySaveFolderPath);

			AppPolicyManager factory = null;
			if (policySaveFolderInfo.Exists)
			{
				factory = AppPolicyManager.Load(policySaveFolderInfo);
			}
			else
			{
				policySaveFolderInfo.Create();

				factory = AppPolicyManager.CreateNew(policySaveFolderInfo);

				// Note: デフォルトで配置するPolicyの準備
			}

			return factory;
		}

		protected override void ConfigureContainer()
		{
			base.ConfigureContainer();


			// リアクションモニターのインスタンスを生成＆DIコンテナに登録
			this.Container.RegisterInstance(InitializeMonitorModel());

			// アプリ起動ポリシー管理のインスタンスを生成＆DIコンテナに登録
			var appLaunchManager = InitializeAppLaunchAction();
			AppLaunchReactiveAction.SetAppPolicyFactory(appLaunchManager);
			this.Container.RegisterInstance<IAppPolicyManager>(appLaunchManager);

			var ea = new EventAggregator();
			this.Container.RegisterInstance<IEventAggregator>(ea);
		}

		protected override void InitializeShell()
		{
			base.InitializeShell();

			
			App.Current.MainWindow = (Window)this.Shell;

#if DEBUG
			App.Current.MainWindow.Show();
#endif
			
		}

		protected override void ConfigureModuleCatalog()
		{
			base.ConfigureModuleCatalog();
			ModuleCatalog moduleCatalog = (ModuleCatalog)this.ModuleCatalog;

			moduleCatalog.AddModule(typeof(Modules.Monitor.MonitorModule));
			moduleCatalog.AddModule(typeof(Modules.Main.MainModule));
			moduleCatalog.AddModule(typeof(Modules.AppPolicy.AppPolicyModule));
			moduleCatalog.AddModule(typeof(Modules.Settings.SettingsModule));
			moduleCatalog.AddModule(typeof(Modules.About.AboutModule));


		}

		protected override void InitializeModules()
		{
			base.InitializeModules();
		}
	}
}
