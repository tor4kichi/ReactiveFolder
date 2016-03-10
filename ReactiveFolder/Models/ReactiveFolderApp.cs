using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Properties;
using System.IO;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.Util;
using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolderStyles.Models;
using Prism.Events;
using ReactiveFolder.Models.History;

namespace ReactiveFolder.Models
{
	public class ReactiveFolderApp : BindableBase
	{

		public const AppPageType InitialPage = AppPageType.AppPolicyManage;


		public const string APP_POLICY_FOLDER_NAME = "app_policy";
		public const string REACTION_FOLDER_NAME = "reaction";
		public const string UPDATE_RECORD_FOLDER_NAME = "update_record";
		public const string INSTANT_ACTION_FOLDER_NAME = "instant_action";
		public const string INSTANT_ACTION_TEMP_FOLDER_NAME = "instant_action_temp";
		public const string HISTORY_FOLDER_NAME = "history";

		public static readonly string DefaultGlobalSettingSavePath =
			new DirectoryInfo(
				Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					"ReactiveFolder"
				)
			).FullName;


		public static readonly string AppDataSaveFolder =
			new DirectoryInfo(
				Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"ReactiveFolder"
				)
			).FullName;

		public static readonly string AppPolicySecuritySavePath =
			Path.Combine(
				AppDataSaveFolder,
				"authorized.json"
				);

		private AppPageType _PageType;
		public AppPageType PageType
		{
			get
			{
				return _PageType;
			}
			set
			{
				SetProperty(ref _PageType, value);
			}
		}



		public ReactiveFolderSettings Settings { get; private set; }



		public AppPolicyManager AppPolicyManager { get; private set; }

		public FileUpdateRecordManager UpdateRecordManager { get; private set; }

		public FolderReactionMonitorModel ReactionMonitor { get; private set; }


		public InstantActionManager InstantActionManager { get; private set; }

		public PageManager PageManager { get; private set; }

		public HistoryManager HistoryManager { get; private set; }


		public ReactiveFolderApp(IEventAggregator ea)
		{
			PageManager = new PageManager(ea);

			// AppData
			var appDataSaveFolder = new DirectoryInfo(AppDataSaveFolder);
			if (false == appDataSaveFolder.Exists)
			{
				appDataSaveFolder.Create();
			}



			Settings = new ReactiveFolderSettings();

			LoadGlobalSettings();


			var appPolicySaveFolder = Path.Combine(Settings.SaveFolder, APP_POLICY_FOLDER_NAME);
			AppPolicyManager = InitializeAppLaunchAction(appPolicySaveFolder);

			var updateRecordSaveFolder = Path.Combine(Settings.SaveFolder, UPDATE_RECORD_FOLDER_NAME);
			UpdateRecordManager = InitializeFileUpdateRecordManager(updateRecordSaveFolder);

			var historySaveFolder = Path.Combine(Settings.SaveFolder, HISTORY_FOLDER_NAME);
			HistoryManager = new HistoryManager(historySaveFolder, Settings.HistoryAvailableStorageSizeMB);



			// Note: ReactionMonitorの前提条件
			// AppPolicy / UpdateRecord / HistoryManager
			var reactionSaveFolder = Path.Combine(Settings.SaveFolder, REACTION_FOLDER_NAME);
			ReactionMonitor = InitializeMonitorModel(reactionSaveFolder, HistoryManager);
			ReactionMonitor.DefaultInterval = TimeSpan.FromSeconds(Settings.DefaultMonitorIntervalSeconds);


			InstantActionManager = new InstantActionManager(AppPolicyManager);
			InstantActionManager.SaveFolder = Path.Combine(Settings.SaveFolder, INSTANT_ACTION_FOLDER_NAME);
			InstantActionManager.TempSaveFolder = Path.Combine(Settings.SaveFolder, INSTANT_ACTION_TEMP_FOLDER_NAME);

		}


		public void OpenInitialPage()
		{
			PageManager.OpenPage(InitialPage);
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
			var security = new AppPolicy.AppPolicySecurity(AppPolicySecuritySavePath);


			var policySaveFolderInfo = new DirectoryInfo(policySaveFolderPath);

			AppPolicyManager appPolicyManager = null;
			if (policySaveFolderInfo.Exists)
			{
				appPolicyManager = AppPolicyManager.Load(policySaveFolderInfo, security);
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


		static FolderReactionMonitorModel InitializeMonitorModel(string monitorSaveFolderPath, IHistoryManager historyManager)
		{
			return new FolderReactionMonitorModel(new DirectoryInfo(monitorSaveFolderPath), historyManager);
		}


	}


	
}
