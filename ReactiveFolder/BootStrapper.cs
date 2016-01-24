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

namespace ReactiveFolder
{
	class Bootstrapper : UnityBootstrapper
	{
		public static readonly string MonitorSettingsSaveFolderPath =
			Path.Combine(
//				System.Windows.Forms.Application.LocalUserAppDataPath,
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				"ReactiveFolder/"
			);


		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}


		private async Task<FolderReactionMonitorModel> InitializeMonitorModel()
		{
			var monitorSaveFolderPath = Properties.Settings.Default.MonitorDataSaveFolderPath;
			if (false == Directory.Exists(monitorSaveFolderPath))
			{
				monitorSaveFolderPath = MonitorSettingsSaveFolderPath;
				Properties.Settings.Default.MonitorDataSaveFolderPath = MonitorSettingsSaveFolderPath;
				Properties.Settings.Default.Save();
			}

			var model = await FolderReactionMonitorModel.LoadOrCreate(new DirectoryInfo(MonitorSettingsSaveFolderPath));

			return model;
		}

		protected override void ConfigureContainer()
		{
			base.ConfigureContainer();

			var modelInitTask = InitializeMonitorModel();
			modelInitTask.Wait();
			this.Container.RegisterInstance(modelInitTask.Result);
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
