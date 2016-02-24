using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.AppPolicy;
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
		private IAppPolicyManager _AppPolicyManager;
		public AppLaunchReactiveAction Action { get; private set; }

		public IEnumerable<AppPolicyViewModel> AppList { get; private set; }

		public ReactiveProperty<AppPolicyViewModel> AppPolicyVM { get; private set; }

		public ReactiveProperty<string> AppName { get; private set; }


		public AppLaunchActionViewModel(ActionsEditViewModel editVM, FolderReactionModel reactionModel, AppLaunchReactiveAction appAction, IAppPolicyManager appPolicyManager)
			 : base(reactionModel)
		{
			EditVM = editVM;
			_AppPolicyManager = appPolicyManager;
			Action = appAction;

			 // TODO: AppListではなく前段の出力拡張子による実行可能な変換オプション
			/*
			// 現在のアクションの前段にあるFilter、またはActionが出力するファイルタイプ・拡張子によってAppListの内容をフィルタする
			var prevFolderItemOutputer = reactionModel.GetPreviousFolderItemOutputer(Action);
			var filters = prevFolderItemOutputer.GetFilters();

			AppList = appPolicyManager.Policies
				// 前段の出力タイプと、
				.Where(x => x.CheckCanProcessPartOfSupport(prevFolderItemOutputer))
				.Select(x => new AppPolicyViewModel(Action, x));
			*/

			
			AppPolicyVM = new ReactiveProperty<AppPolicyViewModel>();
			
			var currentAppPolicy = _AppPolicyManager.FromAppGuid(Action.AppGuid);
			if (currentAppPolicy != null)
			{
				var vm = AppList.SingleOrDefault(x => x.AppGuid == Action.AppGuid);
				if (vm != null)
				{
					AppPolicyVM.Value = vm;
				}
			}

			


			







			AppName = Action.ObserveProperty(x => x.AppGuid)
				.Select(x => _AppPolicyManager.FromAppGuid(Action.AppGuid)?.AppName ?? "???")
				.ToReactiveProperty();

			
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
						// TODO: アクションの削除 確認ダイアログ
						EditVM.RemoveAction(this);
					}));
			}
		}

	}


	public class AppPolicyViewModel : BindableBase, IDisposable
	{
		public static readonly AppPolicyViewModel InvalidAppPolicyVM = new AppPolicyViewModel();

		public AppLaunchReactiveAction ActionModel { get; private set; }
		public ApplicationPolicy AppPolicy { get; private set; }

		public string AppName { get; private set; }

		public Guid AppGuid { get; private set; }

		public ReadOnlyReactiveCollection<AppPolicyOptionViewModel> AppOptions { get; private set; }


		public CompositeDisposable _CompositeDisposable;

		internal AppPolicyViewModel()
		{
			AppName = "?????";
		}

		public AppPolicyViewModel(AppLaunchReactiveAction actionModel, ApplicationPolicy appPolicy)
		{
			this.ActionModel = actionModel;
			this.AppPolicy = appPolicy;
			AppName = AppPolicy.AppName;
			AppGuid = AppPolicy.Guid;

			_CompositeDisposable = new CompositeDisposable();


			AppOptions = AppPolicy.OptionDeclarations.ToReadOnlyReactiveCollection(x => 
				new AppPolicyOptionViewModel(x)
			);


		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}
	}



	public class AppPolicyOptionViewModel : BindableBase
	{
		public AppOptionDeclarationBase Declaration { get; private set; }

		public AppPolicyOptionViewModel(AppOptionDeclarationBase decl)
		{
			Declaration = decl;
		}




	}



	
}
