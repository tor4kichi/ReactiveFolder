using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Timings
{
	[DataContract]
	public class TimerReactiveTiming : ReactiveTimingBase
	{
		[DataMember]
		public DateTime Time { get; set; }

		[DataMember]
		public TimeSpan Span { get; set; }




		public TimerReactiveTiming()
		{
			Time = DateTime.Now;
			Span = TimeSpan.FromHours(1);
		}

		public TimerReactiveTiming(DateTime nextTime, TimeSpan span)
		{
			Time = nextTime;
			Span = span;
		}




		public override ValidationResult Validate()
		{
			return ValidationResult.Valid;
		}




		private bool CheckTimingAndUpdateNextTime()
		{
			// 指定時間を過ぎていれば実行
			var timingIsNow = Time < DateTime.Now;

			if (timingIsNow)
			{
				// 現在時刻ですでに指定時間を迎えている状態を解消する必要がある
				// （久しぶりにアプリを立ち上げた時とか）
				// Spanの間隔を保ったまま次に実行すべき時間を求める
				var subTime = DateTime.Now.Subtract(Time);
				var needSpanCount = (int)Math.Ceiling(subTime.TotalSeconds / Span.TotalSeconds);

				if (needSpanCount > 1)
				{
					System.Diagnostics.Debug.WriteLine($"TimerReactiveTiming: 前回時刻({Time})から現在時刻({DateTime.Now})までの {needSpanCount} 回分のチェックをスキップしました。");
				}

				Time = Time.Add(TimeSpan.FromSeconds(needSpanCount * Span.TotalSeconds));
			}

			return timingIsNow;
		}



		public override IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev)
		{
			return prev.Where(_ => CheckTimingAndUpdateNextTime());
		}

		
	}

}
