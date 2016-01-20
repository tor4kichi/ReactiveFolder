using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveFolder.Model
{
	// TODO: 監視タスクが正常に動作しているかのチェック、実行保証

	
	// 設定は別に保存する

	public class FolderReactionMonitorModel : IDisposable
	{
		private CompositeDisposable MonitorTaskCompositeDisposable;

		public ObservableCollection<FolderReactionGroupModel> ReactionGroups { get; private set; }
		public TimeSpan Interval { get; private set; }

		public FolderReactionMonitorModel()
		{
			MonitorTaskCompositeDisposable = null;

			ReactionGroups = new ObservableCollection<FolderReactionGroupModel>();
		}



		public bool IsRunning
		{
			get
			{
				return MonitorTaskCompositeDisposable != null
					&& ReactionGroups.Count != 0;

			}
		}



		public void Start(TimeSpan monitorIntervalTime)
		{
			if (IsRunning)
			{
				return;
			}


			// 監視タスク始動準備
			MonitorTaskCompositeDisposable = new CompositeDisposable();


			var monitorStream = Observable.Timer(TimeSpan.FromSeconds(0), monitorIntervalTime);

			foreach (var group in ReactionGroups)
			{
				group.Generate(monitorStream)
#if !DEBUG
					.Subscribe()
#else
					.Subscribe(x =>
					{
						System.Diagnostics.Debug.WriteLine(x.Path);
					})
#endif
					.AddTo(this.MonitorTaskCompositeDisposable);
					
			}

			Interval = monitorIntervalTime;
		}

		public void Exit()
		{
			if (false == IsRunning)
			{
				return;
			}

			MonitorTaskCompositeDisposable?.Dispose();
			MonitorTaskCompositeDisposable = null;
		}

		public void Dispose()
		{
			MonitorTaskCompositeDisposable?.Dispose();
			MonitorTaskCompositeDisposable = null;
		}





#if DEBUG

		public BehaviorSubject<long> Debug_Start()
		{
			if (IsRunning)
			{
				Exit();
			}


			// 監視タスク始動準備
			MonitorTaskCompositeDisposable = new CompositeDisposable();


			var subject = new BehaviorSubject<long>(0);

			foreach (var group in ReactionGroups)
			{
				group.Generate(subject)
					.Subscribe(x =>
					{
						System.Diagnostics.Debug.WriteLine(x.Path);
					})
					.AddTo(this.MonitorTaskCompositeDisposable);

			}

			Interval = TimeSpan.MaxValue;

			return subject;
		}

#endif
	}
}
