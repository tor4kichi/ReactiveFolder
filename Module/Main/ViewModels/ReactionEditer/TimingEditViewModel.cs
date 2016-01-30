using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class TimingEditViewModel : ReactionEditViewModelBase, IDisposable
	{
		protected CompositeDisposable _CompositeDisposable { get; private set; }

		public List<TimingViewModelBase> TimingVMs { get; private set; }



		private ReactiveProperty<bool> _IsValid;
		public override ReactiveProperty<bool> IsValid
		{
			get
			{
				return _IsValid;
			}
		}





		public TimingEditViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			_CompositeDisposable = new CompositeDisposable();



			_IsValid = Reaction.ObserveProperty(x => x.IsTimingsValid)
				.ToReactiveProperty();




			TimingVMs = new List<TimingViewModelBase>();

			TimingVMs.Add(new FileUpdateTimingViewModel(Reaction));
			TimingVMs.Add(new TimerTimingViewModel(Reaction));



		}


		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}


	}

}
