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
	public class DestinationViewModelBase : BindableBase, IDisposable
	{
		public FolderReactionModel ReactionModel { get; private set; }

		protected CompositeDisposable _CompositeDisposable { get; private set; }




		public ReactiveProperty<string> OutputNamePattern { get; private set; }



		public DestinationViewModelBase(FolderReactionModel reactionModel)
		{
			ReactionModel = reactionModel;
			_CompositeDisposable = new CompositeDisposable();

			OutputNamePattern = reactionModel.Destination.ToReactivePropertyAsSynchronized(x => x.OutputNamePattern)
				.AddTo(_CompositeDisposable);


		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}
	}
}
