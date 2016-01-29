using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using Modules.Main.Views;
using ReactiveFolder.Model.Filters;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.IO;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using ReactiveFolder.Model.Timings;
using ReactiveFolder.Model.Actions;
using ReactiveFolder.Model.Destinations;
using System.Threading.Tasks;


namespace Modules.Main.ViewModels.ReactionEditer
{
	public class SameInputDestinationViewModel : DestinationViewModelBase
	{
		SameInputReactiveDestination Destination;

		public ReadOnlyReactiveProperty<string> OutputPathSample { get; private set; }


		public SameInputDestinationViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			Destination = reactionModel.Destination as SameInputReactiveDestination;


			OutputPathSample = Observable.CombineLatest(
					ReactionModel.ObserveProperty(x => x.WorkFolder).Select(x => x.FullName)
					, OutputNamePattern
				)
				.Select(x => {
					return Path.Combine(x[0], x[1]);
				})
				.ToReadOnlyReactiveProperty()
				.AddTo(_CompositeDisposable);
		}
	}
}
