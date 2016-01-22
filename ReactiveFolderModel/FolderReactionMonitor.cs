using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

	[Serializable]
	public class FolderReactionMonitorModel : BindableBase, IDisposable
	{
		[DataMember]
		public ObservableCollection<FolderReactionGroupModel> ReactionGroups { get; private set; }

		[DataMember]
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

		private IDisposable MonitorMasterDisposer;

		private BehaviorSubject<long> RemoteTrigger;

		public FolderReactionMonitorModel()
		{
			Interval = TimeSpan.FromMinutes(15);
			ReactionGroups = new ObservableCollection<FolderReactionGroupModel>();
		}



		public bool IsRunning
		{
			get
			{
				return MonitorMasterDisposer != null &&
					ReactionGroups.Count != 0;

			}
		}

		public void CheckNow()
		{
			RemoteTrigger?.OnNext(0);
		}

		public void Start(Action<ReactiveStreamContext> subscribe)
		{
			// 既に走っている監視処理を終了させる
			Exit();


			// 監視対象が存在しなければ切り上げる
			if (ReactionGroups.Count == 0)
			{
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
			var reactionGroupStreams = ReactionGroups
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
		}

		public void Start()
		{
			Start(null);
		}

		public void Exit()
		{
			MonitorMasterDisposer?.Dispose();
			MonitorMasterDisposer = null;

			RemoteTrigger = null;
		}

		public void Dispose()
		{
			MonitorMasterDisposer?.Dispose();
			MonitorMasterDisposer = null;

			RemoteTrigger = null;
		}
	}
}
