using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Modules.Main.ViewModels.ReactionEditer;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.AppPolicy;
using ReactiveFolder.Models.History;
using ReactiveFolder.Models.Timings;
using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels
{

	public enum ReactionFilterType
	{
		Files,
		Folder,

		Unknown
	}


	public class ReactionEditControlViewModel : PageViewModelBase, IDisposable
	{
		public FolderReactionManagePageViewModel PageVM { get; private set; }
		public FolderReactionModel Reaction { get; private set; }
		private IAppPolicyManager _AppPolicyManager;
		public IHistoryManager HistoryManager { get; private set; }
		private CompositeDisposable _CompositeDisposable;

		public ReactiveProperty<ReactionViewModel> ReactionVM { get; private set; }

		public ReactiveProperty<bool> IsNeedSave { get; private set; }

		private IDisposable CanSaveSubscriber;


		public ReactionEditControlViewModel(FolderReactionManagePageViewModel pageVM, IRegionManager regionManager, IFolderReactionMonitorModel monitor, IAppPolicyManager appPolicyManager, IHistoryManager historyManager, FolderReactionModel reaction)
			: base(regionManager, monitor)
		{
			PageVM = pageVM;
			Reaction = reaction;
			_AppPolicyManager = appPolicyManager;
			HistoryManager = historyManager;

			_CompositeDisposable = new CompositeDisposable();

			ReactionVM = new ReactiveProperty<ReactionViewModel>(new ReactionViewModel(Reaction, _AppPolicyManager))
				.AddTo(_CompositeDisposable);


			IsNeedSave = new ReactiveProperty<bool>(false)
				.AddTo(_CompositeDisposable);

			SaveCommand = IsNeedSave.ToReactiveCommand(false)
					.AddTo(_CompositeDisposable);

			SaveCommand.Subscribe(_ => Save())
				.AddTo(_CompositeDisposable);

			CanSaveSubscriber = Observable.Merge(
				Reaction.ObserveProperty(x => x.IsNeedValidation, false).ToUnit(),
				Reaction.PropertyChangedAsObservable().ToUnit()
				)
				.Subscribe(_ =>
				{
					IsNeedSave.Value = true;
				})
				.AddTo(_CompositeDisposable);
		}


		

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
		}





		



		





		/*

		private DelegateCommand _BackCommand;
		public DelegateCommand BackCommand
		{
			get
			{
				return _BackCommand
					?? (_BackCommand = new DelegateCommand(async () =>
					{
						

						
					}));
			}
		}
		*/
		
		public ReactiveCommand SaveCommand { get; private set; }

		public void Save()
		{
			IsNeedSave.Value = false;

			try
			{
				_MonitorModel.SaveReaction(Reaction);
			}
			catch
			{
				IsNeedSave.Value = true;
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
						var results = Reaction.Execute(forceEnable:true);

						var historyData = new HistoryData();
						historyData.Actions = Reaction.Actions.Select(x => x as AppLaunchReactiveAction).ToArray();
						historyData.ActionSourceFilePath = _MonitorModel.RootFolder.MakeReactionSaveFilePath(Reaction);
						historyData.FileHistories = results;

						HistoryManager.SaveHistory(historyData);
					}
					));
			}
		}

		

		
		

		



		

		


		
	}



	public class ReactionViewModel : BindableBase, IDisposable
	{
		CompositeDisposable _CompositeDisposable;


		FolderReactionModel Reaction;
		IAppPolicyManager _AppPolicyManager;

		public ReactiveProperty<bool> IsReactionValid { get; private set; }

		public ReactiveProperty<string> ReactionWorkName { get; private set; }


		public List<ReactionEditViewModelBase> EditVMList { get; private set; }

		public WorkFolderEditViewModel WorkFolderEditVM { get; private set; }
		public FilterEditViewModel FilterEditVM { get; private set; }
		public ActionsEditViewModel ActionsEditVM { get; private set; }
		public DestinationEditViewModel DestinationEditVM { get; private set; }

		public ReactiveProperty<bool> IsEnable { get; private set; }
		public ReactiveProperty<string> MonitorIntervalSeconds { get; private set; }

		// Reactionmodelを受け取ってVMを生成する

		public ReactionViewModel(FolderReactionModel reaction, IAppPolicyManager appPolicyManager)
		{
			_CompositeDisposable = new CompositeDisposable();
			Reaction = reaction;
			_AppPolicyManager = appPolicyManager;


			IsReactionValid = Reaction.ObserveProperty(x => x.IsValid)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			ReactionWorkName = Reaction.ToReactivePropertyAsSynchronized(x => x.Name)
				.AddTo(_CompositeDisposable);

			WorkFolderEditVM = new WorkFolderEditViewModel(Reaction);
			FilterEditVM = new FilterEditViewModel(Reaction);
			ActionsEditVM = new ActionsEditViewModel(Reaction, _AppPolicyManager);
			DestinationEditVM = new DestinationEditViewModel(Reaction);


			EditVMList = new List<ReactionEditViewModelBase>()
			{
				WorkFolderEditVM,
				FilterEditVM,
				ActionsEditVM,
				DestinationEditVM
			};

			IsEnable = Reaction.ToReactivePropertyAsSynchronized(x => x.IsEnable)
				.AddTo(_CompositeDisposable);

			// see@ http://stackoverflow.com/questions/1833830/timespan-parse-time-format-hhmmss
			// https://msdn.microsoft.com/en-us/library/ee372286.aspx
			MonitorIntervalSeconds = Reaction.ToReactivePropertyAsSynchronized(
				x => x.CheckInterval
				, convert: (timespan) => ((int)timespan.TotalSeconds).ToString()
				, convertBack: (seconds) => TimeSpan.FromSeconds(int.Parse(seconds))
				, ignoreValidationErrorValue: true
			)
			.AddTo(_CompositeDisposable);

			MonitorIntervalSeconds.SetValidateNotifyError(text =>
			{
				int temp;
				if (false == int.TryParse(text, out temp))
				{
					return "Number Only";
				}

				return null;
			});

			


		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
		}
	}

	


}
