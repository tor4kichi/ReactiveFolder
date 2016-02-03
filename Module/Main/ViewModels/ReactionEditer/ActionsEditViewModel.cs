using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using ReactiveFolder.Model.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class ActionsEditViewModel : ReactionEditViewModelBase
	{
		private ReactiveProperty<bool> _IsValid;
		public override ReactiveProperty<bool> IsValid
		{
			get
			{
				return _IsValid;
			}
		}



		public ReadOnlyReactiveCollection<AppLaunchActionViewModel> Actions { get; private set; }


		public ActionsEditViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			_IsValid = Reaction.ObserveProperty(x => x.IsActionsValid)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			Actions = Reaction.Actions.ToReadOnlyReactiveCollection(x => 
				new AppLaunchActionViewModel(Reaction, x as AppLaunchReactiveAction)
				);
		}



	}
}
