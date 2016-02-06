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


		private ReactiveProperty<bool> _IsValid;
		public override ReactiveProperty<bool> IsValid
		{
			get
			{
				return _IsValid;
			}
		}



		public ReadOnlyReactiveCollection<AppLaunchActionViewModel> Actions { get; private set; }


		public ActionsEditViewModel(FolderReactionModel reactionModel, IAppPolicyManager appPolicyManager)
			: base(reactionModel)
		{
			_AppPolicyManager = appPolicyManager;

			_IsValid = Reaction.ObserveProperty(x => x.IsActionsValid)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			Actions = Reaction.Actions.ToReadOnlyReactiveCollection(x => 
				new AppLaunchActionViewModel(Reaction, x as AppLaunchReactiveAction, _AppPolicyManager)
				);
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
						
						using (var tempVM = new AppLaunchActionViewModel(Reaction, appLaunchAction, _AppPolicyManager))
						{
							var result = await OpenAppLaunchActionEditDialog(tempVM);
							if (result != null && ((bool)result) == true)
							{
								Reaction.AddAction(appLaunchAction);
							}
						}

					}));
			}
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
