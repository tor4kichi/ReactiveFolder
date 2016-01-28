using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using ReactiveFolder.Model.Timings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class TimingViewModelTemplate<TIMING_MODEL> : TimingViewModelBase
		where TIMING_MODEL : ReactiveTimingBase, new()
	{

		public ReactiveProperty<bool> IsEnable { get; private set; }

		private TIMING_MODEL _CachedModel;


		public TimingViewModelTemplate(FolderReactionModel reactionModel)
			: base(reactionModel)
		{

			var timingModel = ReactionModel.Timings.SingleOrDefault(x => x is TIMING_MODEL);
			var hasTypedTimingModel = timingModel != null;
			IsEnable = new ReactiveProperty<bool>(hasTypedTimingModel);

			_CachedModel = timingModel as TIMING_MODEL;

			IsEnable.Subscribe(x =>
			{
				if (x)
				{
					if (_CachedModel == null)
					{
						_CachedModel = new TIMING_MODEL();
					}
					ReactionModel.AddTiming(_CachedModel);
				}
				else
				{
					ReactionModel.RemoveTiming(_CachedModel);
				}
			})
			.AddTo(_CompositeDisposable);
		}



	}
}
