using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using ReactiveFolder.Model.Util;
using System.IO;
using System.Diagnostics;

namespace ReactiveFolder
{
	[RunInstaller(true)]
	public partial class ShortcutInstallCustomAction : System.Configuration.Install.Installer
	{
		public const string APP_NAME = @"ReactiveFolder.exe";
		public const string LINK_NAME = @"Launch ReactiveFolder";


		public ShortcutInstallCustomAction()
		{
			InitializeComponent();
		}

		public override void Install(IDictionary stateSaver)
		{
			base.Install(stateSaver);

		}

		protected override void OnAfterInstall(IDictionary savedState)
		{
			if (Context.Parameters["DesktopShortcut"] == "1")
			{
				CreateApplicationShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
			}

			if (Context.Parameters["AddToStartup"] == "1")
			{
				CreateApplicationShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
			}
		}


		private void CreateApplicationShortcut(string targetFolder)
		{
			var appDir = Context.Parameters["dir"];
			var appPath = Path.Combine(appDir, APP_NAME);
			ShortcutHelper.CreateShortcut(appPath, targetFolder, LINK_NAME);
		}








		public override void Rollback(IDictionary savedState)
		{
			base.Rollback(savedState);

			DeleteApplicationShortcurt(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
			DeleteApplicationShortcurt(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);

			DeleteApplicationShortcurt(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
			DeleteApplicationShortcurt(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
		}

		

		private void DeleteApplicationShortcurt(string targetFolder)
		{
			var linkPath = ShortcutHelper.MakeShortcutPath(targetFolder, LINK_NAME);

			var fileInfo = new FileInfo(linkPath);
			if(fileInfo.Exists)
			{
				fileInfo.Delete();
			}

		}
	}
}
