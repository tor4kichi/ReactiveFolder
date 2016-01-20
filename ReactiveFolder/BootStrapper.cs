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

namespace ReactiveFolder
{
	class Bootstrapper : UnityBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void InitializeShell()
		{
			base.InitializeShell();

			var monitor = new FolderReactionMonitorModel();
			Container.RegisterInstance(monitor);

			App.Current.MainWindow = (Window)this.Shell;

			//			App.Current.MainWindow.Show();
		}
	}
}
