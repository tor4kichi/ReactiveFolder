using ReactiveFolder.Model;
using ReactiveFolder.Model.Timings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{

	// TODO: タイマー起動タイミングの設定UIを追加

	public class TimerTimingViewModel : TimingViewModelTemplate<TimerReactiveTiming>
	{
		public TimerTimingViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{

		}
	}
}
