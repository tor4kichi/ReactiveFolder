using Modules.Main.ViewModels.ReactionEditer;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using ReactiveFolder.Model.AppPolicy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Modules.Main.ViewModels
{

	public enum ReactionFilterType
	{
		Files,
		Folder,

		Unknown
	}

	
	public class ReactionEditerPageViewModel : PageViewModelBase, INavigationAware, IDisposable
	{
		private FolderReactionModel Reaction;
		private IAppPolicyManager _AppPolicyManager;
		private CompositeDisposable _CompositeDisposable;

		public IRegionNavigationService NavigationService;


		public bool IsReactionValid { get; private set; }

		public ReactiveProperty<string> ReactionWorkName { get; private set; }

		public ReactiveProperty<WorkFolderEditViewModel> WorkFolderEditVM { get; private set; }
		public ReactiveProperty<FilterEditViewModel> FilterEditVM { get; private set; }
		public ReactiveProperty<ActionsEditViewModel> ActionsEditVM { get; private set; }
		public ReactiveProperty<DestinationEditViewModel> DestinationEditVM { get; private set; }

		public ReactiveProperty<bool> IsEnableSave { get; private set; }


		public ReactionEditerPageViewModel(IRegionManager regionManager, FolderReactionMonitorModel monitor, IAppPolicyManager appPolicyManager)
			: base(regionManager, monitor)
		{
			_AppPolicyManager = appPolicyManager;

			IsReactionValid = false;

			ReactionWorkName = new ReactiveProperty<string>("");

			WorkFolderEditVM = new ReactiveProperty<WorkFolderEditViewModel>();
			FilterEditVM = new ReactiveProperty<FilterEditViewModel>();
			ActionsEditVM = new ReactiveProperty<ActionsEditViewModel>();
			DestinationEditVM = new ReactiveProperty<DestinationEditViewModel>();

			IsEnableSave = new ReactiveProperty<bool>(true);
		}


		private void Initialize()
		{
			// initialize with this.Reaction
			CleanUpReactionSubscribe();

			_CompositeDisposable = new CompositeDisposable();

			ReactionWorkName.Value = Reaction.Name;

			Reaction.ObserveProperty(x => x.WorkFolder)
				.Subscribe(x =>
				{
					WorkFolderEditVM.Value?.Dispose();
					FilterEditVM.Value?.Dispose();
					ActionsEditVM.Value?.Dispose();
					DestinationEditVM.Value?.Dispose();

					WorkFolderEditVM.Value = new WorkFolderEditViewModel(Reaction);
					FilterEditVM.Value = new FilterEditViewModel(Reaction);
					ActionsEditVM.Value = new ActionsEditViewModel(Reaction, _AppPolicyManager);
					DestinationEditVM.Value = new DestinationEditViewModel(Reaction);
				})
				.AddTo(_CompositeDisposable);

			Reaction.ObserveProperty(x => x.IsValid)
				.Subscribe(x =>
				{
					IsReactionValid = x;
					OnPropertyChanged(nameof(IsReactionValid));
				})
				.AddTo(_CompositeDisposable);
			

			IsEnableSave.Value = true;
		}


		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{

		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			NavigationService = navigationContext.NavigationService;
			Reaction = base.ReactionModelFromNavigationParameters(navigationContext.Parameters);

			Initialize();
		}


		private void CleanUpReactionSubscribe()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}

		public void Dispose()
		{
			CleanUpReactionSubscribe();

			ReactionWorkName?.Dispose();
			IsEnableSave?.Dispose();

			WorkFolderEditVM.Value?.Dispose();
			FilterEditVM.Value?.Dispose();
			ActionsEditVM.Value?.Dispose();
			DestinationEditVM.Value?.Dispose();
		}

		private DelegateCommand _BackCommand;
		public DelegateCommand BackCommand
		{
			get
			{
				return _BackCommand
					?? (_BackCommand = new DelegateCommand(() =>
					{
						if (NavigationService.Journal.CanGoBack)
						{
							NavigationService.Journal.GoBack();
						}
						else
						{
							this.NavigationToFolderListPage(_MonitorModel.RootFolder);
						}
					}));
			}
		}



		private DelegateCommand _SaveCommand;
		public DelegateCommand SaveCommand
		{
			get
			{
				return _SaveCommand
					?? (_SaveCommand = new DelegateCommand(() =>
					{
						IsEnableSave.Value = false;

						try
						{
							Task.Run(async () =>
							{
								_MonitorModel.SaveReaction(Reaction);
								await Task.Delay(500);
							})
							.ContinueWith(_ => { IsEnableSave.Value = true; });
						}
						finally
						{

						}
					}));
			}
		}

		private DelegateCommand _TestCommand;
		public DelegateCommand TestCommand
		{
			get
			{
				return _TestCommand
					?? (_TestCommand = new DelegateCommand(() =>
					{
						Reaction.Test();
//						Reaction.Start();
//						Reaction.CheckNow();
					}
					));
			}
		}



		
		



	}
}
