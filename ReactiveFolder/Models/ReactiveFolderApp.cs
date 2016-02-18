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
		public const string APP_POLICY_FOLDER_NAME = "app_policy";
		public const string REACTION_FOLDER_NAME = "reaction";
		public const string UPDATE_RECORD_FOLDER_NAME = "update_record";

		public static readonly string DefaultGlobalSettingSavePath =
			new DirectoryInfo(
			Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					"ReactiveFolder"
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


			var appPolicySaveFolder = Path.Combine(Settings.SaveFolder, APP_POLICY_FOLDER_NAME);
			AppPolicyManager = InitializeAppLaunchAction(appPolicySaveFolder);



			var updateRecordSaveFolder = Path.Combine(Settings.SaveFolder, UPDATE_RECORD_FOLDER_NAME);
			UpdateRecordManager = InitializeFileUpdateRecordManager(updateRecordSaveFolder);


			// Note: AppPolicyとUpdateRecordが ReactionMonitor の前提条件となるため、ReactionMonitorを最後に初期化

			var reactionSaveFolder = Path.Combine(Settings.SaveFolder, REACTION_FOLDER_NAME);
			ReactionMonitor = InitializeMonitorModel(reactionSaveFolder);
			ReactionMonitor.DefaultInterval = TimeSpan.FromSeconds(Settings.DefaultMonitorIntervalSeconds);



		}



		public void SaveGlobalSettings()
		{
			
		}

		public void LoadGlobalSettings()
		{
			Settings.Load();


			// パスチェック
			if (String.IsNullOrEmpty(Settings.SaveFolder) ||
				false == Directory.Exists(Settings.SaveFolder))
			{
				Settings.SaveFolder = DefaultGlobalSettingSavePath;
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
