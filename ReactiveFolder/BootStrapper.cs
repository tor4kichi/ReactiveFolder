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
using ReactiveFolderStyles;
using ReactiveFolderStyles.Models;
using ReactiveFolder.Models.History;

namespace ReactiveFolder
{
	class Bootstrapper : UnityBootstrapper
	{
		public ReactiveFolderApp ReactiveFolderApp { get; private set; }
		public string[] Args { get; private set; }

		public Bootstrapper(string[] args)
		{
			Args = args;
		}


		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}


		protected override void ConfigureContainer()
		{
			base.ConfigureContainer();

			// イベント管理
			var ea = new EventAggregator();
			this.Container.RegisterInstance<IEventAggregator>(ea);


			ReactiveFolderApp = new ReactiveFolderApp(ea);

			this.Container.RegisterInstance<IHistoryManager>(ReactiveFolderApp.HistoryManager);



			// リアクションモニターのインスタンスを生成＆DIコンテナに登録
			this.Container.RegisterInstance<IFolderReactionMonitorModel>(ReactiveFolderApp.ReactionMonitor);

			// アプリ起動ポリシー管理の〃
			this.Container.RegisterInstance<IAppPolicyManager>(ReactiveFolderApp.AppPolicyManager);

			// ファイル更新タイミング記録管理の〃
			this.Container.RegisterInstance<IFileUpdateRecordManager>(ReactiveFolderApp.UpdateRecordManager);

			this.Container.RegisterInstance<IInstantActionManager>(ReactiveFolderApp.InstantActionManager);

			this.Container.RegisterInstance<IReactiveFolderSettings>(ReactiveFolderApp.Settings);

			this.Container.RegisterInstance<PageManager>(ReactiveFolderApp.PageManager);


			this.Container.RegisterInstance<ReactiveFolderApp>(ReactiveFolderApp);

			

		}

		protected override void InitializeShell()
		{
			base.InitializeShell();

			App.Current.MainWindow = (Window)this.Shell;
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
			moduleCatalog.AddModule(typeof(Modules.InstantAction.InstantActionModule));
			moduleCatalog.AddModule(typeof(Modules.History.HistoryModule));


		}

		protected override void InitializeModules()
		{
			base.InitializeModules();

			var filePaths = Args.Where(x => false == String.IsNullOrWhiteSpace(x))
				.Where(x => System.IO.Path.IsPathRooted(x))
				.Where(x => System.IO.File.Exists(x))
				.ToArray();

			if (filePaths.Count() > 0)
			{
				ReactiveFolderApp.PageManager.OpenInstantActionWithDefaultFiles(filePaths);
			}
			else
			{
				ReactiveFolderApp.OpenInitialPage();
			}


			

#if DEBUG
			App.Current.MainWindow.Show();
#endif


		}
	}
}
