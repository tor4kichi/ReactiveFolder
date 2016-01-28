using ReactiveFolder.Model;
using ReactiveFolder.Model.Timings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class FileUpdateTimingViewModel : TimingViewModelTemplate<FileUpdateReactiveTiming>
	{
		public FileUpdateTimingViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{

		}
	}
}
