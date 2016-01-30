using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	abstract public class ReactionEditViewModelBase : BindableBase
	{
		public FolderReactionModel Reaction { get; private set; }

		abstract public ReactiveProperty<bool> IsValid { get; }



		public ReactionEditViewModelBase(FolderReactionModel reactionModel)
		{
			Reaction = reactionModel;
		}


	}
}
