using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveFolder.Model
{

	


	public class MonitorSettings
	{
		public int DefaultIntervalSeconds { get; set; }
	}

	public class FolderReactionMonitorModel : BindableBase, IDisposable
	{

		
		public static FolderReactionMonitorModel LoadOrCreate(DirectoryInfo saveFolder, Func<string, bool> skipReactionFolder = null)
		{
			var model = new FolderReactionMonitorModel(saveFolder);

			model.InitializeSettings();
			model.InitializeReactions();

			return model;
		}


		public const string MONITOR_SETTINGS_FILENAME = "settings.json";



		private DirectoryInfo _SaveFolder;
		public DirectoryInfo SaveFolder
		{
			get
			{
				return _SaveFolder;
			}
			set
			{
				if (false == value.Exists)
				{
					value.Create();
				}

				_SaveFolder = value;
			}
		}



		public FolderModel RootFolder { get; private set; }

		private TimeSpan _DefaultInterval;
		public TimeSpan DefaultInterval
		{
			get
			{
				return _DefaultInterval;
			}
			set
			{
				SetProperty(ref _DefaultInterval, value);
			}
		}		

		private FolderReactionMonitorModel(DirectoryInfo saveFolder)
		{
			SaveFolder = saveFolder;

			DefaultInterval = TimeSpan.FromMinutes(15);
		}



		public async void Save()
		{
			SaveSettings();
		}


		#region private Settings

		private FileInfo MakeSettingFileInfo()
		{
			// SaveFolderが存在しなければ作成を試みる?

			return new FileInfo(
				Path.Combine(
					this.SaveFolder.FullName,
					MONITOR_SETTINGS_FILENAME
					)
				);
		}


		private void InitializeSettings()
		{
			// saveFolder内のsettings.jsonを読む
			var settingSaveFileInfo = MakeSettingFileInfo();

			if (settingSaveFileInfo.Exists)
			{
				var settings = FileSerializeHelper.LoadAsync<MonitorSettings>(settingSaveFileInfo);

				if (settings == null)
				{
					settingSaveFileInfo.Delete();
					return;
				}

				this.DefaultInterval = TimeSpan.FromSeconds(settings.DefaultIntervalSeconds);
			}
			else
			{
				SaveSettings();
			}
		}


		#endregion



		#region public Settings

		public void SaveSettings()
		{
			var settings = new MonitorSettings();

			settings.DefaultIntervalSeconds = (int)this.DefaultInterval.TotalSeconds;

			var settingSaveFileInfo = MakeSettingFileInfo();

			FileSerializeHelper.Save(settingSaveFileInfo, settings);
		}



		#endregion


		public void SaveReaction(FolderReactionModel reaction)
		{
			var folder = FindReactionParentFolder(reaction);
			if (folder != null)
			{
				folder.SaveReaction(reaction);
			}
			else
			{
				// 削除されたリアクション、またはフォルダが削除されている
			}
		}


		public FolderReactionModel FindReaction(Guid guid)
		{
			return RootFolder.FindReaction(guid);
		}

		public FolderModel FindReactionParentFolder(Guid guid)
		{
			return RootFolder.FindReactionParent(guid);
		}

		public FolderModel FindReactionParentFolder(FolderReactionModel model)
		{
			return RootFolder.FindReactionParent(model);
		}

		// TODO: 再帰的にフォルダをめぐってFolderModelを構築する

		private void InitializeReactions()
		{
			RootFolder = FolderModel.LoadFolder(SaveFolder);
		}




		#region Monitor Manage

		public void Start()
		{
			// 既に走っている監視処理を終了させる
			Exit();


			RootFolder.Start();
		}



		public void Exit()
		{
			RootFolder.Exit();
		}

		public void Dispose()
		{
			Exit();
		}




		#endregion


	}
}
