using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using ReactiveFolder.Model.Actions;
using ReactiveFolder.Model.AppPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class AppLaunchActionViewModel : ActionViewModelBase
	{
		private IAppPolicyManager _AppPolicyManager;
		private AppLaunchReactiveAction _Action;

		public ReadOnlyReactiveCollection<string> AppList { get; private set; }

		public ReadOnlyReactiveCollection<string> AppArgumentList { get; private set; }

		public ReactiveProperty<string> AppName { get; private set; }

		public ReactiveProperty<string> ArgumentName { get; private set; }

		public AppLaunchActionViewModel(FolderReactionModel reactionModel, AppLaunchReactiveAction appAction, IAppPolicyManager appPolicyManager)
			 : base(reactionModel)
		{
			_AppPolicyManager = appPolicyManager;
			_Action = appAction;

			AppList = appPolicyManager.Policies
				.ToReadOnlyReactiveCollection(x => x.AppName);


			AppName = appAction.ToReactivePropertyAsSynchronized(x => x.AppName);

			ArgumentName = appAction.ToReactivePropertyAsSynchronized(x => x.AppArgumentName);

			AppArgumentList = AppName
				.Where(x => false == String.IsNullOrEmpty(x))
				.Select(x => appPolicyManager.FromAppName(x))
				.SelectMany(x => x.AppParams.Select(y => y.Name))
				.ToReadOnlyReactiveCollection();
		}


		private DelegateCommand<string> _SelectAppCommand;
		public DelegateCommand<string> SelectAppCommand
		{
			get
			{
				return _SelectAppCommand
					?? (_SelectAppCommand = new DelegateCommand<string>((x) =>
					{
					}));
			}
		}

		private DelegateCommand<string> _SelectAppOptionCommand;
		public DelegateCommand<string> SelectAppOptionCommand
		{
			get
			{
				return _SelectAppOptionCommand
					?? (_SelectAppOptionCommand = new DelegateCommand<string>((x) =>
					{
					}));
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
						// TODO: 確認ダイアログ
						ReactionModel.RemoveAction(this._Action);
					}));
			}
		}

	}


	public class AppPolicyViewModel : BindableBase
	{
		public ApplicationPolicy AppPolicy { get; private set; }

		public AppPolicyViewModel()
		{

		}
	}

	public class AppPolicyArgumentViewModel : BindableBase
	{

	}
}
