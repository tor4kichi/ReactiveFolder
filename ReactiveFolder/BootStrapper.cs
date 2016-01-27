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
		private void InitializeAppLaunchAction()
		{
			// Note: 使用ポリシーはアプリローカル空間に保存する
			var policySaveFolderPath = ActionAppPolicyFolderPath;

			var policySaveFolderInfo = new DirectoryInfo(policySaveFolderPath);

			AppPolicyFactory factory = null;
			if (policySaveFolderInfo.Exists)
			{
				factory = AppPolicyFactory.Load(policySaveFolderInfo);
			}
			else
			{
				policySaveFolderInfo.Create();

				factory = AppPolicyFactory.CreateNew(policySaveFolderInfo);

				// Note: デフォルトで配置するPolicyの準備
			}

			AppLaunchReactiveAction.SetAppPolicyFactory(factory);
		}

		protected override void ConfigureContainer()
		{
			base.ConfigureContainer();




			this.Container.RegisterInstance(InitializeMonitorModel());



			InitializeAppLaunchAction();
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
		}
	}
}
