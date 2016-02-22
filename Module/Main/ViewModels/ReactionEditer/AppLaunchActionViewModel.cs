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
		public ReactiveProperty<string> OutputFormatName { get; private set; }


		public AppLaunchActionViewModel(ActionsEditViewModel editVM, FolderReactionModel reactionModel, AppLaunchReactiveAction appAction, IAppPolicyManager appPolicyManager)
			 : base(reactionModel)
		{
			EditVM = editVM;
			_AppPolicyManager = appPolicyManager;
			Action = appAction;

			// 現在のアクションの前段にあるFilter、またはActionが出力するファイルタイプ・拡張子によってAppListの内容をフィルタする
			var prevFolderItemOutputer = reactionModel.GetPreviousFolderItemOutputer(Action);
			var filters = prevFolderItemOutputer.GetFilters();

			AppList = appPolicyManager.Policies
				// 前段の出力タイプと、
				.Where(x => x.CheckCanProcessPartOfSupport(prevFolderItemOutputer))
				.Select(x => new AppPolicyViewModel(Action, x));
			

			
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

			// App変更時のモデルへの書き戻し
			AppPolicyVM
				.Where(x => x != null)
				.Where(x => Action.AppGuid != x.AppGuid)
				.Subscribe(x =>
			{
				Action.AppGuid = x.AppGuid;

				var appPolicy = _AppPolicyManager.FromAppGuid(x.AppGuid);
				Action.AppOutputFormatId = appPolicy
					.AppOutputFormats.FirstOrDefault()?.Id ?? AppOutputFormat.IgnoreArgumentId;
			});


			







			AppName = Action.ObserveProperty(x => x.AppGuid)
				.Select(x => _AppPolicyManager.FromAppGuid(Action.AppGuid)?.AppName ?? "???")
				.ToReactiveProperty();

			OutputFormatName =
				Observable.CombineLatest(
					Action.ObserveProperty(x => x.AppGuid),
					Action.ObserveProperty(x => x.AppOutputFormatId),
					(x, y) =>
					{
						var appPolicy = _AppPolicyManager.FromAppGuid(x);
						var outputFormat = appPolicy?.FindOutputFormat(y) ?? null;
						return outputFormat?.Name ?? "-";
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

		public ReadOnlyReactiveCollection<AppPolicyOutputFormatViewModel> AppOutputFormatList { get; private set; }


		public ReactiveProperty<AppPolicyOutputFormatViewModel> OutputFormatVM { get; private set; }


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


			// App変更時のOutputFormatリストの更新
			AppOutputFormatList = AppPolicy.AppOutputFormats.ToReadOnlyReactiveCollection(x =>
				new AppPolicyOutputFormatViewModel(x)
			)
			.AddTo(_CompositeDisposable);


			OutputFormatVM = new ReactiveProperty<AppPolicyOutputFormatViewModel>(
				AppOutputFormatList.SingleOrDefault(x => x.AppOutputFormat.Id == ActionModel.AppOutputFormatId)
				, ReactivePropertyMode.DistinctUntilChanged
				);


			OutputFormatVM.Subscribe(x =>
			{
				ActionModel.AppOutputFormatId = x.AppOutputFormat.Id;
			});


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



	public class AppPolicyOutputFormatViewModel : BindableBase
	{
		public static readonly AppPolicyOutputFormatViewModel InvalidAppArgumentVM = new AppPolicyOutputFormatViewModel();



		public AppOutputFormat AppOutputFormat { get; private set; }

		public string OutputFormatName { get; private set; }


		public AppPolicyOutputFormatViewModel()
		{
			this.OutputFormatName = "???";
		}

		public AppPolicyOutputFormatViewModel(AppOutputFormat outputFormat)
		{
			this.AppOutputFormat = outputFormat;

			this.OutputFormatName = this.AppOutputFormat?.Name ?? "???";
		}
	}
}
