using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using Modules.Main.Views;
using ReactiveFolder.Model.Filters;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.IO;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using ReactiveFolder.Model.Timings;
using ReactiveFolder.Model.Actions;
using ReactiveFolder.Model.Destinations;
using System.Threading.Tasks;

using Modules.Main.ViewModels.ReactionEditer;

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

		private CompositeDisposable _CompositeDisposable;

		public IRegionNavigationService NavigationService;

		public bool IsReactionValid { get; private set; }

		public ReactiveProperty<string> ReactionWorkName { get; private set; }

		public ReactiveProperty<WorkFolderEditViewModel> WorkFolderEditVM { get; private set; }
		public ReactiveProperty<FilterEditViewModel> FilterEditVM { get; private set; }
		public ReactiveProperty<TimingEditViewModel> TimingEditVM { get; private set; }
		public ReactiveProperty<ActionsEditViewModel> ActionsEditVM { get; private set; }
		public ReactiveProperty<DestinationEditViewModel> DestinationEditVM { get; private set; }

		public ReactiveProperty<bool> IsEnableSave { get; private set; }


		public ReactionEditerPageViewModel(IRegionManager regionManager, IRegionNavigationService navService, FolderReactionMonitorModel monitor)
			: base(regionManager, monitor)
		{
			IsReactionValid = false;

			ReactionWorkName = new ReactiveProperty<string>("");

			WorkFolderEditVM = new ReactiveProperty<WorkFolderEditViewModel>();
			FilterEditVM = new ReactiveProperty<FilterEditViewModel>();
			TimingEditVM = new ReactiveProperty<TimingEditViewModel>();
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
					TimingEditVM.Value?.Dispose();
					ActionsEditVM.Value?.Dispose();
					DestinationEditVM.Value?.Dispose();

					WorkFolderEditVM.Value = new WorkFolderEditViewModel(Reaction);
					FilterEditVM.Value = new FilterEditViewModel(Reaction);
					TimingEditVM.Value = new TimingEditViewModel(Reaction);
					ActionsEditVM.Value = new ActionsEditViewModel(Reaction);
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
			TimingEditVM.Value?.Dispose();
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
							this.NavigationToFolderListPage();
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
