using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class TimingEditViewModel : BindableBase, IDisposable
	{
		public FolderReactionModel ReactionModel;

		protected CompositeDisposable _CompositeDisposable { get; private set; }



		public List<TimingViewModelBase> TimingVMs { get; private set; }

		public TimingEditViewModel(FolderReactionModel reactionModel)
		{
			ReactionModel = reactionModel;
			_CompositeDisposable = new CompositeDisposable();


			TimingVMs = new List<TimingViewModelBase>();

			TimingVMs.Add(new FileUpdateTimingViewModel(ReactionModel));
			TimingVMs.Add(new TimerTimingViewModel(ReactionModel));
		}


		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}
	}

}
