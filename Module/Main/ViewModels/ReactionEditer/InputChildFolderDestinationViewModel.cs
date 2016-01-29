using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using ReactiveFolder.Model.Destinations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class InputChildFolderDestinationViewModel : DestinationViewModelBase
	{
		ChildReactiveDestination Destination;

		public ReactiveProperty<string> ChildFolderName { get; private set; }

		public ReadOnlyReactiveProperty<string> OutputPathSample { get; private set; }


		public InputChildFolderDestinationViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			Destination = reactionModel.Destination as ChildReactiveDestination;

			ChildFolderName = Destination.ToReactivePropertyAsSynchronized(x => x.ChildFolderName)
				.AddTo(_CompositeDisposable);

			OutputPathSample = Observable.CombineLatest(
					ReactionModel.ObserveProperty(x => x.WorkFolder).Select(x => x.FullName)
					, ChildFolderName
					, OutputNamePattern
				)
				.Select(x => {
					return Path.Combine(x[0], x[1], x[2]);
				})
				.ToReadOnlyReactiveProperty()
				.AddTo(_CompositeDisposable);
		}
	}
}
