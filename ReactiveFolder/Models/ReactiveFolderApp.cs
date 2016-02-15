﻿using System;
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
	public class ReactiveFolderApp : IReactiveFolderApp
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








		public FolderReactionMonitorModel ReactionMonitor { get; private set; }

		public AppPolicyManager AppPolicyManager { get; private set; }

		public ReactiveFolderGlobalSettings Settings { get; private set; }






		public ReactiveFolderApp()
		{
			Settings = new ReactiveFolderGlobalSettings();

			LoadGlobalSettings();

			// ReactionMonitorを初期化
			ReactionMonitor = InitializeMonitorModel(Settings.ReactionSaveFolder);
			ReactionMonitor.DefaultInterval = TimeSpan.FromSeconds(Settings.DefaultMonitorIntervalSeconds);

			// AppPolicyManagerを初期化
			AppPolicyManager = InitializeAppLaunchAction(Settings.AppPolicySaveFolder);
		}



		public void SaveGlobalSettings()
		{
			Properties.Settings.Default.ReactionSaveFolder = Settings.ReactionSaveFolder;
			Properties.Settings.Default.AppPolicySaveFolder = Settings.AppPolicySaveFolder;
			Properties.Settings.Default.DefaultMonitorIntervalSeconds = Settings.DefaultMonitorIntervalSeconds;

			Properties.Settings.Default.Save();
		}

		public void LoadGlobalSettings()
		{
			Settings.ReactionSaveFolder = Properties.Settings.Default.ReactionSaveFolder;
			Settings.AppPolicySaveFolder = Properties.Settings.Default.AppPolicySaveFolder;
			Settings.DefaultMonitorIntervalSeconds = Properties.Settings.Default.DefaultMonitorIntervalSeconds;


			// パスチェック

			if (String.IsNullOrWhiteSpace(Settings.ReactionSaveFolder))
			{
				Settings.ReactionSaveFolder = DefaultReactionSavePath;

				Properties.Settings.Default.ReactionSaveFolder = Settings.ReactionSaveFolder;
				Properties.Settings.Default.Save();
			}

			if (String.IsNullOrWhiteSpace(Settings.AppPolicySaveFolder))
			{
				Settings.AppPolicySaveFolder = DefaultAppPolicySavePath;

				Properties.Settings.Default.AppPolicySaveFolder = Settings.AppPolicySaveFolder;
				Properties.Settings.Default.Save();
			}
		}


		static FolderReactionMonitorModel InitializeMonitorModel(string monitorSaveFolderPath)
		{
			return new FolderReactionMonitorModel(new DirectoryInfo(monitorSaveFolderPath));
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
		
	}
}