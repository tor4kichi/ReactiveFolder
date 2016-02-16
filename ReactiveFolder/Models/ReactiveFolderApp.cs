using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Properties;
using System.IO;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.Util;

namespace ReactiveFolder.Models
{
	public class ReactiveFolderApp 
	{
		
		public static readonly string DefaultGlobalSettingSavePath =
			new DirectoryInfo(
			Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					"ReactiveFolder"
				)
			).FullName;

		public static readonly string DefaultReactionSavePath =
			new DirectoryInfo(
			Path.Combine(
					DefaultGlobalSettingSavePath,
					"reaction"
				)
			).FullName;

		public static readonly string DefaultAppPolicySavePath =
			new DirectoryInfo(
			Path.Combine(
					DefaultGlobalSettingSavePath,
					"apppolicy"
				)
			).FullName;



		public static readonly string DefaultUpdateRecordSavePath =
			new DirectoryInfo(
			Path.Combine(
					DefaultGlobalSettingSavePath,
					"update_record"
				)
			).FullName;




		public ReactiveFolderSettings Settings { get; private set; }



		public AppPolicyManager AppPolicyManager { get; private set; }

		public FileUpdateRecordManager UpdateRecordManager { get; private set; }

		public FolderReactionMonitorModel ReactionMonitor { get; private set; }



		public ReactiveFolderApp()
		{
			Settings = new ReactiveFolderSettings();

			LoadGlobalSettings();


			
			AppPolicyManager = InitializeAppLaunchAction(Settings.AppPolicySaveFolder);



			UpdateRecordManager = InitializeFileUpdateRecordManager(Settings.UpdateRecordSaveFolder);


			// Note: AppPolicyとUpdateRecordが ReactionMonitor の前提条件となるため、ReactionMonitorを最後に初期化


			ReactionMonitor = InitializeMonitorModel(Settings.ReactionSaveFolder);
			ReactionMonitor.DefaultInterval = TimeSpan.FromSeconds(Settings.DefaultMonitorIntervalSeconds);



		}



		public void SaveGlobalSettings()
		{
			
		}

		public void LoadGlobalSettings()
		{
			Settings.Load();


			// パスチェック

			if (String.IsNullOrWhiteSpace(Settings.AppPolicySaveFolder))
			{
				Settings.AppPolicySaveFolder = DefaultAppPolicySavePath;
			}

			if (String.IsNullOrWhiteSpace(Settings.UpdateRecordSaveFolder))
			{
				Settings.UpdateRecordSaveFolder = DefaultUpdateRecordSavePath;
			}

			if (String.IsNullOrWhiteSpace(Settings.ReactionSaveFolder))
			{
				Settings.ReactionSaveFolder = DefaultReactionSavePath;
			}

			Settings.Save();
		}


		
		/// <summary>
		/// 外部アプリの使用ポリシーのファイルを読み込んでAppPolicyFactoryを初期化する
		/// </summary>
		static AppPolicyManager InitializeAppLaunchAction(string policySaveFolderPath)
		{
			var policySaveFolderInfo = new DirectoryInfo(policySaveFolderPath);

			AppPolicyManager appPolicyManager = null;
			if (policySaveFolderInfo.Exists)
			{
				appPolicyManager = AppPolicyManager.Load(policySaveFolderInfo);
			}
			else
			{
				policySaveFolderInfo.Create();

				appPolicyManager = AppPolicyManager.CreateNew(policySaveFolderInfo);

				// Note: デフォルトで配置するPolicyの準備
			}

			AppLaunchReactiveAction.SetAppPolicyManager(appPolicyManager);


			return appPolicyManager;
		}
		

		static FileUpdateRecordManager InitializeFileUpdateRecordManager(string recordSaveFolderPath)
		{
			var manager = new FileUpdateRecordManager();
			manager.SetSaveFolder(new DirectoryInfo(recordSaveFolderPath));

			Timings.FileUpdateReactiveTiming.SetFileUpdateRecordManager(manager);

			return manager;
		}


		static FolderReactionMonitorModel InitializeMonitorModel(string monitorSaveFolderPath)
		{
			return new FolderReactionMonitorModel(new DirectoryInfo(monitorSaveFolderPath));
		}


	}
}
