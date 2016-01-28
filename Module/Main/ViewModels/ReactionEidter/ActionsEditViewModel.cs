using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class ActionsEditViewModel : BindableBase
	{

		public FolderReactionModel ReactionModel;
		public ActionsEditViewModel(FolderReactionModel reactionModel)
		{
			ReactionModel = reactionModel;
		}
	}
}
