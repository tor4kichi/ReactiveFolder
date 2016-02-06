using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public enum ValidationState
	{
		WaitFirstValidate,

		Valid,
		Invalid
	}

	abstract public class ReactionEditViewModelBase : BindableBase, IDisposable
	{
		public FolderReactionModel Reaction { get; private set; }

		public ReactiveProperty<ValidationState> ValidateState { get; private set; }
		public string Title { get; private set; }
		public ReactiveProperty<bool> IsValid { get; private set; }

		protected CompositeDisposable _CompositeDisposable;

		public ReactionEditViewModelBase(string title, FolderReactionModel reactionModel)
		{
			Title = title;
			Reaction = reactionModel;
			_CompositeDisposable = new CompositeDisposable();


			IsValid = new ReactiveProperty<bool>(false)
				.AddTo(_CompositeDisposable);

			ValidateState = IsValid
				.Select(x => x ? ValidationState.Valid : ValidationState.Invalid)
				.ToReactiveProperty(ValidationState.WaitFirstValidate)
				.AddTo(_CompositeDisposable);
			
		}

		public virtual void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}
	}
}
