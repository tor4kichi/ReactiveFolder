using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IWshRuntimeLibrary;


namespace ReactiveFolder.Model.Util
{
	public static class ShortcutHelper
	{
		

		public static void CreateShortcut(string sourcePath, string targetFolder, string linkName, string iconLocation = null)
		{
			//作成するショートカットのパス
			string shortcutPath = MakeShortcutPath(targetFolder, linkName);

			//WshShellを作成
			Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
			IWshShell shell = Activator.CreateInstance(t) as IWshShell;
			//WshShortcutを作成
			var shortcut = shell.CreateShortcut(shortcutPath) as IWshShortcut;


			//リンク先
			shortcut.TargetPath = sourcePath;

			//アイコンのパス
			if (iconLocation != null)
			{
				shortcut.IconLocation = iconLocation;
			}
			//その他のプロパティも同様に設定できるため、省略
			
			//ショートカットを作成
			shortcut.Save();

			//後始末
			System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
			System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
		}	

		public static string MakeShortcutPath(string targetFolder, string linkName)
		{
			return System.IO.Path.Combine(
			   targetFolder,
			   $"{linkName}.lnk");
		}
	}
}
