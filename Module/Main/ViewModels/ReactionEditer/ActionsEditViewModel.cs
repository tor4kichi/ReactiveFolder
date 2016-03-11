using MaterialDesignThemes.Wpf;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.AppPolicy;
using ReactiveFolder.Models.Util;
using ReactiveFolderStyles.DialogContent;
using ReactiveFolderStyles.Models;
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


		public ActionsEditViewModel(FolderReactionModel reactionModel, PageManager pageManager, IAppPolicyManager appPolicyManager)
			: base(pageManager, reactionModel)
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
						return new AppLaunchActionViewModel(this, Reaction, appLaunchAction);
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


		protected override IEnumerable<string> GetValidateError()
		{
			ValidationResult validateResult = Reaction.ValidateActions();
			return validateResult.Messages;
		}


		public async Task<object> ShowSelectAppOptionDialog(AppPolicySelectDialogContentViewModel vm)
		{ 
			var view = new AppPolicySelectDialogContent()
			{
				DataContext = vm
			};

			return await DialogHost.Show(view, "ReactionEditCommonDialogHost");
		}


		private DelegateCommand _SelectApplicationCommand;
		public DelegateCommand SelectApplicationCommand
		{
			get
			{
				return _SelectApplicationCommand
					?? (_SelectApplicationCommand = new DelegateCommand(async () =>
					{
						var items = _AppPolicyManager.Policies.Select(x => new AppPolicySelectItem()
						{
							AppName = x.AppName,
							AppGuid = x.Guid
						});

						var dialogVM = new AppPolicySelectDialogContentViewModel(items);

						var result = await ShowSelectAppOptionDialog(dialogVM);
						if (result != null && ((bool)result) == true)
						{
							var actionModel = new AppLaunchReactiveAction();
							actionModel.AppGuid = dialogVM.SelectedItem.AppGuid;




							Reaction.AddAction(actionModel);

							// VM上のコレクション操作によってActions.CollectionChangedAsObservableが反応して、
							// VM→モデルへとコレクションの状態が同期される

							var actionVM = new AppLaunchActionViewModel(this, Reaction, actionModel);
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
