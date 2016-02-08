﻿using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using ReactiveFolder.Model.Actions;
using ReactiveFolder.Model.AppPolicy;
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

		public ReadOnlyReactiveCollection<AppPolicyViewModel> AppList { get; private set; }

		public ReactiveProperty<AppPolicyViewModel> AppPolicyVM { get; private set; }

		public ReactiveProperty<string> AppName { get; private set; }
		public ReactiveProperty<string> ArgumentName { get; private set; }


		public AppLaunchActionViewModel(ActionsEditViewModel editVM, FolderReactionModel reactionModel, AppLaunchReactiveAction appAction, IAppPolicyManager appPolicyManager)
			 : base(reactionModel)
		{
			EditVM = editVM;
			_AppPolicyManager = appPolicyManager;
			Action = appAction;

			AppList = appPolicyManager.Policies
				.ToReadOnlyReactiveCollection(x => new AppPolicyViewModel(Action, x));



			
			// TODO: 未選択状態の番兵となるAppPolicyVMを使って安全に表示を行いたい

			
			AppPolicyVM = new ReactiveProperty<AppPolicyViewModel>();
			
			var currentAppPolicy = _AppPolicyManager.FromAppGuid(Action.AppGuid);
			if (currentAppPolicy != null)
			{
				var vm = AppList.Single(x => x.AppGuid == Action.AppGuid);
				AppPolicyVM.Value = vm;
			}

			// App変更時のモデルへの書き戻し
			AppPolicyVM
				.Where(x => x != null)
				.Where(x => Action.AppGuid != x.AppGuid)
				.Subscribe(x =>
			{
				Action.AppGuid = x.AppGuid;

				var appPolicy = _AppPolicyManager.FromAppGuid(x.AppGuid);
				Action.AppArgumentId = appPolicy
					.AppParams.FirstOrDefault()?.Id ?? AppArgument.IgnoreArgumentId;
			});


			







			AppName = Action.ObserveProperty(x => x.AppGuid)
				.Select(x => _AppPolicyManager.FromAppGuid(Action.AppGuid)?.AppName ?? "???")
				.ToReactiveProperty();

			ArgumentName =
				Observable.CombineLatest(
					Action.ObserveProperty(x => x.AppGuid),
					Action.ObserveProperty(x => x.AppArgumentId),
					(x, y) =>
					{
						var appPolicy = _AppPolicyManager.FromAppGuid(x);
						var arg = appPolicy?.FindArgument(y) ?? null;
						return arg?.Name ?? "-";
					})
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
						// TODO: 確認ダイアログ
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

		public ReadOnlyReactiveCollection<AppPolicyArgumentViewModel> AppArgumentList { get; private set; }


		public ReactiveProperty<AppPolicyArgumentViewModel> ArgumentVM { get; private set; }


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


			// App変更時のArgumentリストの更新
			AppArgumentList = AppPolicy.AppParams.ToReadOnlyReactiveCollection(x =>
				new AppPolicyArgumentViewModel(x)
			)
			.AddTo(_CompositeDisposable);


			ArgumentVM = new ReactiveProperty<AppPolicyArgumentViewModel>(
				AppArgumentList.SingleOrDefault(x => x.AppArgument.Id == ActionModel.AppArgumentId)
				, ReactivePropertyMode.DistinctUntilChanged
				);


			ArgumentVM.Subscribe(x =>
			{
				ActionModel.AppArgumentId = x.AppArgument.Id;
			});

		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}
	}

	public class AppPolicyArgumentViewModel : BindableBase
	{
		public static readonly AppPolicyArgumentViewModel InvalidAppArgumentVM = new AppPolicyArgumentViewModel();



		public AppArgument AppArgument { get; private set; }

		public string ArgumentName { get; private set; }


		public AppPolicyArgumentViewModel()
		{
			this.ArgumentName = "???";
		}

		public AppPolicyArgumentViewModel(AppArgument arg)
		{
			this.AppArgument = arg;

			this.ArgumentName = this.AppArgument?.Name ?? "???";
		}
	}
}
