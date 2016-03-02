using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.AppPolicy;
using ReactiveFolderStyles.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class AppLaunchActionViewModel : ActionViewModelBase
	{
		public ActionsEditViewModel EditVM { get; private set; }

		public AppLaunchReactiveAction Action { get; private set; }
		public ApplicationPolicy AppPolicy { get; private set; }
		public AppOptionInstance OptionInstance { get; private set; }

		public Guid AppGuid { get; private set; }

		public string AppName { get; private set; }

		public string OptionName { get; private set; }

		public List<AppOptionValueViewModel> OptionValues { get; private set; }


		public AppLaunchActionViewModel(ActionsEditViewModel editVM, FolderReactionModel reactionModel, AppLaunchReactiveAction appAction, AppOptionInstance optionInstance)
			 : base(reactionModel)
		{
			EditVM = editVM;
			Action = appAction;
			OptionInstance = optionInstance;

			AppPolicy = appAction.AppPolicy;
			if (AppPolicy != null)
			{
				AppName = AppPolicy.AppName;
				AppGuid = AppPolicy.Guid;

				OptionName = OptionInstance.OptionDeclaration.Name;

				OptionValues = OptionInstance.FromAppOptionInstance()
					.ToList();
			}
			else
			{
				AppName = "<App not found>";
				OptionName = "";
				OptionValues = new List<AppOptionValueViewModel>();
			}
		}




		private DelegateCommand _RemoveActionCommand;
		public DelegateCommand RemoveActionCommand
		{
			get
			{
				return _RemoveActionCommand
					?? (_RemoveActionCommand = new DelegateCommand(() =>
					{
						// TODO: アクションの削除 確認ダイアログ
						EditVM.RemoveAction(this);
					}));
			}
		}
	}


	


	


	



	
}
