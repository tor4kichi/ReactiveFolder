using MaterialDesignThemes.Wpf;
using Microsoft.Practices.Prism.Commands;
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
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class ActionsEditViewModel : ReactionEditViewModelBase
	{
		// TODO: アプリケーションのリストとAppArgumentの辞書

		private IAppPolicyManager _AppPolicyManager;


	


		/// <summary>
		/// アクションVMのコレクション
		/// いつもはMをVに渡すだけなのでReadOnlyReactiveCollectionを利用するが
		/// Actionsの入れ替えをView側で行うため、変更可能なObservableCollectionを利用している。
		/// </summary>
		public ObservableCollection<AppLaunchActionViewModel> Actions { get; private set; }


		public ActionsEditViewModel(FolderReactionModel reactionModel, IAppPolicyManager appPolicyManager)
			: base(@"Actions", reactionModel)
		{
			_AppPolicyManager = appPolicyManager;

			Reaction.ObserveProperty(x => x.IsActionsValid)
				.Subscribe(x => IsValid.Value = x)
				.AddTo(_CompositeDisposable);


			// TODO: CollectionChangedをマージしてReactiveCollectionにする方法を使ってまとめる
			Actions = new ObservableCollection<AppLaunchActionViewModel>(
				Reaction.Actions.Select(
					x => new AppLaunchActionViewModel(this, Reaction, x as AppLaunchReactiveAction, _AppPolicyManager)
					)
				);

			Actions.CollectionChangedAsObservable()
				.Subscribe(itemPair => 
				{
					var onceRemoveItems = Reaction.Actions.ToArray();
					foreach (var onceRemoveItem in onceRemoveItems)
					{
						Reaction.RemoveAction(onceRemoveItem);
					}
					
					foreach(var reAdditem in Actions)
					{
						Reaction.AddAction(reAdditem.Action);
					}
				});
		}


		public async Task<object> OpenAppLaunchActionEditDialog(AppLaunchActionViewModel vm)
		{
			var view = new Views.ReactionEditer.AppLaunchActionEditDialogContent()
			{
				DataContext = vm
			};

			return await DialogHost.Show(view, "AppLuanchActionEditDialog");
		}


		private DelegateCommand _AddAppLaunchActionCommand;
		public DelegateCommand AddAppLaunchActionCommand
		{
			get
			{
				return _AddAppLaunchActionCommand
					?? (_AddAppLaunchActionCommand = new DelegateCommand(async () =>
					{
						var appLaunchAction = new AppLaunchReactiveAction();
						
						using (var tempVM = new AppLaunchActionViewModel(this, Reaction, appLaunchAction, _AppPolicyManager))
						{
							var result = await OpenAppLaunchActionEditDialog(tempVM);
							if (result != null && ((bool)result) == true)
							{
								// Actions.CollectionChangedAsObservableでVM上のコレクション操作に反応してモデル側の
								// Actionコレクションへの操作を行っている
								// このため、先にモデル側を更新してからVM側を更新しないといけない。

								// この順番を逆にしてしまうと、VM→モデルに同期した後、さらにモデル上に追加する形になってしまうので変更は注意。

								// 1. 先にモデルを更新
								Reaction.AddAction(appLaunchAction);

								// 2. VM上のコレクション操作によってActions.CollectionChangedAsObservableが反応して、
								// VM→モデルへとコレクションの状態が同期される
								Actions.Add(tempVM);
							}
						}

					}));
			}
		}

		public void RemoveAction(AppLaunchActionViewModel actionVM)
		{
			Actions.Remove(actionVM);
			Reaction.RemoveAction(actionVM.Action);
		}


		// Note: AppLaunchActionViewModel以外を編集することになったときは
		// ActionViewModelBaseをコマンドパラメータの型に直して
		// コマンド処理中にVMの型をチェックして開くべきダイアログコンテンツを切り替える
		private DelegateCommand<AppLaunchActionViewModel> _EditAppLaunchActionCommand;
		public DelegateCommand<AppLaunchActionViewModel> EditAppLaunchActionCommand
		{
			get
			{
				return _EditAppLaunchActionCommand
					?? (_EditAppLaunchActionCommand = new DelegateCommand<AppLaunchActionViewModel>(async (vm) =>
					{
						await OpenAppLaunchActionEditDialog(vm);
					}));
			}
		}



		
	}
}
