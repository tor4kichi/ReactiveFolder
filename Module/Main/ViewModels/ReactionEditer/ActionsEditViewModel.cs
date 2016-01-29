using Microsoft.Practices.Prism.Mvvm;
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

		
		public ActionsEditViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			
		}


		protected override bool IsValidateModel()
		{
			return Reaction.ValidateActions().IsValid;
		}
	}
}
