using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	abstract public class ReactionEditViewModelBase : BindableBase, IDisposable
	{
		public FolderReactionModel Reaction { get; private set; }

		abstract public ReactiveProperty<bool> IsValid { get; }

		protected CompositeDisposable _CompositeDisposable;

		public ReactionEditViewModelBase(FolderReactionModel reactionModel)
		{
			Reaction = reactionModel;
			_CompositeDisposable = new CompositeDisposable();
		}

		public virtual void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}
	}
}
