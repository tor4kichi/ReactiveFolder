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
using ReactiveFolder.Models;
using Prism.Modularity;

using ReactiveFolder.Properties;
using ReactiveFolder.Models.AppPolicy;
using Prism.Events;
using ReactiveFolder.Models.Timings;

namespace ReactiveFolder
{
	class Bootstrapper : UnityBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}


		protected override void ConfigureContainer()
		{
			base.ConfigureContainer();

			var app = new ReactiveFolderApp();

			// リアクションモニターのインスタンスを生成＆DIコンテナに登録
			this.Container.RegisterInstance<IFolderReactionMonitorModel>(app.ReactionMonitor);

			// アプリ起動ポリシー管理の〃
			this.Container.RegisterInstance<IAppPolicyManager>(app.AppPolicyManager);

			// ファイル更新タイミング記録管理の〃
			this.Container.RegisterInstance<IFileUpdateRecordManager>(app.UpdateRecordManager);

			this.Container.RegisterInstance<IReactiveFolderSettings>(app.Settings);

			// イベント管理
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
