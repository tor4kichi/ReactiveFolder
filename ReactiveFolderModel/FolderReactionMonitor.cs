using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings.Extensions;
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
	// TODO: 監視タスクが正常に動作しているかのチェック、実行保証

	
	// 設定は別に保存する

	// FolderReactionGroupごとに処理の開始や停止をハンドリングできるようにする

	public class MonitorSettings
	{
		public int MonitorIntervalSeconds { get; set; }
	}

	public class FolderReactionMonitorModel : BindableBase, IDisposable
	{

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



		private ObservableCollection<FolderReactionGroupModel> _ReactionGroups;

		public ReadOnlyObservableCollection<FolderReactionGroupModel> ReactionGroups { get; private set; }

		private TimeSpan _Interval;
		public TimeSpan Interval
		{
			get
			{
				return _Interval;
			}
			set
			{
				SetProperty(ref _Interval, value);
			}
		}

		public TimeSpan RunningInterval { get; private set; }

		public bool IsRunning
		{
			get
			{
				return MonitorMasterDisposer != null &&
					ReactionGroups.Count != 0;

			}
		}




		private IDisposable MonitorMasterDisposer;

		private BehaviorSubject<long> RemoteTrigger;


		public static async Task<FolderReactionMonitorModel> LoadOrCreate(DirectoryInfo saveFolder)
		{
			var model = new FolderReactionMonitorModel(saveFolder);

			await model.InitializeSettings();
			await model.InitializeReactions();

			return model;
		}


		private FolderReactionMonitorModel(DirectoryInfo saveFolder)
		{
			SaveFolder = saveFolder;

			Interval = TimeSpan.FromMinutes(15);
			_ReactionGroups = new ObservableCollection<FolderReactionGroupModel>();
			ReactionGroups = new ReadOnlyObservableCollection<FolderReactionGroupModel>(_ReactionGroups);

		}



		public async void Save()
		{
			await SaveSettings();
			await SaveReactionGroups();
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

		

		private async Task InitializeSettings()
		{
			// saveFolder内のsettings.jsonを読む
			var settingSaveFileInfo = MakeSettingFileInfo();

			if (settingSaveFileInfo.Exists)
			{
				var settings = await Util.FileSerializeHelper.LoadAsync<MonitorSettings>(settingSaveFileInfo);

				this.Interval = TimeSpan.FromSeconds(settings.MonitorIntervalSeconds);
			}
			else
			{
				await SaveSettings();
			}
		}


		#endregion



		#region public Settings

		public async Task SaveSettings()
		{
			var settings = new MonitorSettings();

			settings.MonitorIntervalSeconds = (int)this.Interval.TotalSeconds;

			var settingSaveFileInfo = MakeSettingFileInfo();

			await Util.FileSerializeHelper.Save(settingSaveFileInfo, settings);
		}



		#endregion




		#region private ReactionGroup
		

		private async Task SaveSingleReactionGroup(FolderReactionGroupModel reaction)
		{
			var groupsFolder = SaveFolder;

			var reactionGroupFileName = Path.Combine(groupsFolder.FullName, reaction.Guid.ToString() + ".json");

			var reactionGroupFileInfo = new FileInfo(reactionGroupFileName);
			if (reactionGroupFileInfo.Exists)
			{
				reactionGroupFileInfo.Delete();
			}

			await Util.FileSerializeHelper.Save(reactionGroupFileInfo, reaction);
		}


		private async void DeleteSingleReactionGroup(FolderReactionGroupModel reaction)
		{
			var groupsFolder = SaveFolder;

			var reactionGroupFileName = Path.Combine(groupsFolder.FullName, reaction.Guid.ToString() + ".json");

			var reactionGroupFileInfo = new FileInfo(reactionGroupFileName);
			if (reactionGroupFileInfo.Exists)
			{
				await Task.Run(() => reactionGroupFileInfo.Delete());
			}
		}

		private async Task InitializeReactions()
		{
			var groupsFolder = SaveFolder;

			var files = groupsFolder.EnumerateFiles("*.json")
				.Where(x => x.Name != MONITOR_SETTINGS_FILENAME);

			foreach (var fileInfo in files)
			{
				try
				{
					var groupModel = await Util.FileSerializeHelper.LoadAsync<FolderReactionGroupModel>(fileInfo);

					if (groupModel == null)
					{
						throw new Exception("");
					}
					this._ReactionGroups.Add(groupModel);
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine("FolderReactionGroupModelの読み込みに失敗。");
					System.Diagnostics.Debug.WriteLine(fileInfo.FullName);
				}
			}
		}


		#endregion



		#region public ReactionGroup


		public async Task SaveReactionGroups()
		{
			foreach (var reaction in ReactionGroups)
			{
				await SaveSingleReactionGroup(reaction);
			}
		}

		public FolderReactionGroupModel CreateNewReactionGroup(DirectoryInfo targetDir)
		{
			var groupModel = new FolderReactionGroupModel(targetDir);

			groupModel.Name = targetDir.Name;

			this._ReactionGroups.Add(groupModel);

			return groupModel;
		}

		public async void SaveReactionGroup(Guid reactionGroupGuid)
		{
			var group = ReactionGroups.SingleOrDefault(x => x.Guid == reactionGroupGuid);

			if (group == null)
			{
				throw new Exception();
			}

			await SaveSingleReactionGroup(group);
		}

		public void RevmoeReactionGroup(Guid reactionGroupGuid)
		{
			var group = ReactionGroups.SingleOrDefault(x => x.Guid == reactionGroupGuid);

			if (group == null)
			{
				throw new Exception();
			}

			_ReactionGroups.Remove(group);
			DeleteSingleReactionGroup(group);
		}


		#endregion



		#region Monitor Manage

		public void Exit()
		{
			MonitorMasterDisposer?.Dispose();
			MonitorMasterDisposer = null;

			RemoteTrigger = null;
		}

		public void Dispose()
		{
			Exit();
		}



		public void CheckNow()
		{
			RemoteTrigger?.OnNext(0);
		}

		public void Start(Action<ReactiveStreamContext> subscribe = null)
		{
			// 既に走っている監視処理を終了させる
			Exit();


			// 監視対象が存在しなければ切り上げる
			if (ReactionGroups.Count == 0)
			{
				return;
			}

			// 妥当性チェックをパスしたストリームのみを抽出
			var validReactions = ReactionGroups.Where(x =>
			{
				return false == x.Validate().HasValidationError;
			});

			// 妥当性チェックを一つも通らなかったら監視タスクは不要になる
			if (validReactions.Count() == 0)
			{
				System.Diagnostics.Debug.WriteLine("ストリームを開始させられるReactionGroupがありません。");
#if DEBUG
				System.Diagnostics.Debugger.Break();
#endif
				return;
			}


			// 手動でモニター処理を走らせるためのトリガー用サブジェクト
			RemoteTrigger = new BehaviorSubject<long>(0);


			// モニター処理の起動要因ストリーム
			var monitorTriggersStream =
				Observable.Merge(
					// １．タイマー
					Observable.Timer(TimeSpan.FromSeconds(0), Interval),
					// ２．手動トリガー
					RemoteTrigger
					);

			// 起動要因ストリームを元に
			// ReactionGroupsから処理ストリームに射影
			var reactionGroupStreams = validReactions
				.Select(x => x.Generate(monitorTriggersStream));


			// ReactionGroupsそれぞれの処理ストリームを一本にまとめて
			// Hotストリームに変換
			var mergedStream = Observable
				.Merge(reactionGroupStreams)
				.Publish();


			// 購読が必要なら
			if (subscribe != null)
			{
				mergedStream.Subscribe(subscribe);
			}

			// ストリームを始動させる
			MonitorMasterDisposer = mergedStream.Connect();

			RunningInterval = Interval;

			// Done!
		}



		#endregion


	}
}
