﻿using MaterialDesignThemes.Wpf;
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
	
	public class ActionsEditViewModel : ReactionEditViewModelBase
	{
		private IAppPolicyManager _AppPolicyManager;


	


		/// <summary>
		/// アクションVMのコレクション
		/// いつもはMをVに渡すだけなのでReadOnlyReactiveCollectionを利用するが
		/// Actionsの入れ替えをView側で行うため、変更可能なObservableCollectionを利用している。
		/// </summary>
		public ObservableCollection<AppLaunchActionViewModel> Actions { get; private set; }


		public ActionsEditViewModel(FolderReactionModel reactionModel, IAppPolicyManager appPolicyManager)
			: base(reactionModel)
		{
			_AppPolicyManager = appPolicyManager;

			Reaction.ObserveProperty(x => x.IsActionsValid)
				.Subscribe(x => IsValid.Value = x)
				.AddTo(_CompositeDisposable);


			// TODO: CollectionChangedをマージしてReactiveCollectionにする方法を使ってまとめる
			// Note: AppLaunchActionViewModelをAppOptionValueInstance１つずつに対して作成している
			Actions = new ObservableCollection<AppLaunchActionViewModel>(
				Reaction.Actions.Select(
					x =>
					{
						var appLaunchAction = x as AppLaunchReactiveAction;
						var optionInstance = appLaunchAction.AdditionalOptions[0];
						return new AppLaunchActionViewModel(this, Reaction, appLaunchAction, optionInstance);
					}
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


		public async Task<object> ShowSelectAppOptionDialog(AppOptionSelectDialogViewModel vm)
		{ 
			var view = new Views.DialogContent.AppOptionSelectDialogContent()
			{
				DataContext = vm
			};

			return await DialogHost.Show(view, "ReactionEditCommonDialogHost");
		}


		private DelegateCommand _SelectAppOptionCommand;
		public DelegateCommand SelectAppOptionCommand
		{
			get
			{
				return _SelectAppOptionCommand
					?? (_SelectAppOptionCommand = new DelegateCommand(async () =>
					{
						var dialogVM = new AppOptionSelectDialogViewModel(_AppPolicyManager);

						var result = await ShowSelectAppOptionDialog(dialogVM);
						if (result != null && ((bool)result) == true)
						{
							var opt = dialogVM.SelectedOption.Value;

							var actionModel = new AppLaunchReactiveAction();
							actionModel.AppGuid = opt.AppPolicy.Guid;

							var optionInstance = opt.Declaration.CreateInstance();
							actionModel.AddAppOptionInstance(optionInstance);



							Reaction.AddAction(actionModel);

							// VM上のコレクション操作によってActions.CollectionChangedAsObservableが反応して、
							// VM→モデルへとコレクションの状態が同期される

							var actionVM = new AppLaunchActionViewModel(this, Reaction, actionModel, optionInstance);
							Actions.Add(actionVM);
						}

					}));
			}
		}

		internal void RemoveAction(AppLaunchActionViewModel actionVM)
		{
			Actions.Remove(actionVM);
			Reaction.RemoveAction(actionVM.Action);
		}





		
	}


	public class AppOptionSelectDialogViewModel : BindableBase, IDisposable
	{
		private CompositeDisposable _CompositeDisposable { get; set; }

		public IAppPolicyManager AppPolicyManager { get; private set; }

		public List<AppOptionListItemViewModel> Options { get; private set; }

		public ReactiveProperty<AppOptionListItemViewModel> SelectedOption { get; private set; }

		public ReactiveProperty<bool> IsSelectedOption { get; private set; }

		public AppOptionSelectDialogViewModel(IAppPolicyManager appPolicyManager)
		{
			_CompositeDisposable = new CompositeDisposable();

			AppPolicyManager = appPolicyManager;

			Options = AppPolicyManager.Policies.SelectMany(x => 
			{
				return x.OptionDeclarations
					.Concat(x.OutputOptionDeclarations)
					.Select(y => new AppOptionListItemViewModel(x, y));
			})
			.ToList();

			SelectedOption = new ReactiveProperty<AppOptionListItemViewModel>()
				.AddTo(_CompositeDisposable);

			IsSelectedOption = SelectedOption.Select(x => x != null)
				.ToReactiveProperty(false);
		}

		public void Dispose()
		{
			_CompositeDisposable.Dispose();
		}
	}


	// ダイアログ上に表示する選択肢

	public class AppOptionListItemViewModel : BindableBase
	{
		public ApplicationPolicy AppPolicy { get; private set; }
		public AppOptionDeclarationBase Declaration { get; private set; }

		public string AppName { get; private set; }
		public string OptionName { get; private set; }
		public List<string> PropertyNames { get; private set; }

		public AppOptionListItemViewModel(ApplicationPolicy appPolicy, AppOptionDeclarationBase decl)
		{
			AppPolicy = appPolicy;
			Declaration = decl;

			AppName = AppPolicy.AppName;
			OptionName = Declaration.Name;

			PropertyNames = Declaration
				.UserProperties.Select(x => x.ValiableName)
				.ToList();
		}




	}
}
