using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Timings
{
	public class TimerReactiveTiming : ReactiveTimingBase
	{
		public DateTimeOffset Time { get; set; }
		public TimeSpan Span { get; set; }

		public TimerReactiveTiming(DateTime nextTime, TimeSpan span)
		{
			Time = new DateTimeOffset(nextTime);
		}

		private bool CheckTimingAndUpdateNextTime()
		{
			// 指定時間を過ぎていれば実行
			var timingIsNow = Time < DateTime.Now;

			if (timingIsNow)
			{
				// 正確な次回実行時間の計算
				// 実行間隔 - (現在時刻 - 実行予定時間)
				var overTime = DateTime.Now.Subtract(Time.DateTime);
				var realSpan = Span.Subtract(overTime);
				Time = new DateTimeOffset(Time.DateTime, realSpan);

				while (CheckTimingAndUpdateNextTime()) { }
			}

			return timingIsNow;
		}



		public override IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev)
		{
			return prev.Where(_ =>
			{
				return CheckTimingAndUpdateNextTime();
			});
		}
	}

}
