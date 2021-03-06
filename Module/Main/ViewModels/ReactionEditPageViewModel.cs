﻿using MaterialDesignThemes.Wpf;
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
using ReactiveFolderStyles.Models;
using ReactiveFolderStyles.ViewModels;
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


	public class ReactionEditPageViewModel : PageViewModelBase, IDisposable
	{
		public IFolderReactionMonitorModel Monitor { get; private set; }
		private IAppPolicyManager _AppPolicyManager;
		public IHistoryManager HistoryManager { get; private set; }
		private CompositeDisposable _CompositeDisposable;

		public ReactiveProperty<ReactionViewModel> ReactionVM { get; private set; }


		public ReactionEditPageViewModel(PageManager pageManager, IFolderReactionMonitorModel monitor, IAppPolicyManager appPolicyManager, IHistoryManager historyManager)
			: base(pageManager)
		{
			Monitor = monitor;
			_AppPolicyManager = appPolicyManager;
			HistoryManager = historyManager;

			_CompositeDisposable = new CompositeDisposable();

			ReactionVM = new ReactiveProperty<ReactionViewModel>()
				.AddTo(_CompositeDisposable);

			SaveCommand = new ReactiveCommand();

			SaveCommand.Subscribe(_ => Save())
				.AddTo(_CompositeDisposable);

		}


		

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
		}



		public FolderReactionModel Reaction
		{
			get { return ReactionVM.Value?.Reaction; }
		}


		public override void OnNavigatedTo(NavigationContext navigationContext)
		{
			if (navigationContext.Parameters.Count() > 0)
			{
				if (navigationContext.Parameters.Any(x => x.Key == "guid"))
				{
					try
					{
						var reactionGuid = (Guid)navigationContext.Parameters["guid"];

						var reaction = Monitor.RootFolder.FindReaction(reactionGuid);
						ReactionVM.Value = new ReactionViewModel(reaction, PageManager, _AppPolicyManager);

						PageManager.IsOpenSubContent = true;
					}
					catch
					{
						Console.WriteLine("FolderReactionManagePage: パラメータが不正です。存在するReactionのGuidを指定してください。");
					}
				}

				else if (navigationContext.Parameters.Any(x => x.Key == "filepath"))
				{
					try
					{
						var reactionFilePath = (string)navigationContext.Parameters["filepath"];

						var reaction = Monitor.RootFolder.Reactions
							.SingleOrDefault(x => Monitor.RootFolder.MakeReactionSaveFilePath(x) == reactionFilePath);


						if (reaction == null)
						{
							throw new Exception("use import reaction.");
						}

						ReactionVM.Value = new ReactionViewModel(reaction, PageManager, _AppPolicyManager);
						PageManager.IsOpenSubContent = true;
					}
					catch
					{
						Console.WriteLine("FolderReactionManagePage: パラメータが不正です。存在するReactionのGuidを指定してください。");
					}
				}
			}

		}

		
		public override void OnNavigatedFrom(NavigationContext navigationContext)
		{
			ReactionVM.Value?.Dispose();
			ReactionVM.Value = null;
		}


		public static NavigationParameters CreateOpenReactionParameter(Guid reactionGuid)
		{
			var parameters = new NavigationParameters();

			parameters.Add("guid", reactionGuid);
			return parameters;
		}

		public static NavigationParameters CreateOpenReactionParameter(string filePath)
		{
			var parameters = new NavigationParameters();

			parameters.Add("filepath", filePath);
			return parameters;
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
			try
			{
				Monitor.SaveReaction(Reaction);
				PageManager.ShowInformation($"{Reaction.Name} Saved");
			}
			catch
			{
				PageManager.ShowError($"{Reaction.Name} Failed Save");
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
						historyData.ActionSourceFilePath = Monitor.RootFolder.MakeReactionSaveFilePath(Reaction);
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


		public FolderReactionModel Reaction { get; private set; }
		public PageManager PageManager { get; private set; }
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

		public ReactionViewModel(FolderReactionModel reaction, PageManager pageManager, IAppPolicyManager appPolicyManager)
		{
			_CompositeDisposable = new CompositeDisposable();
			Reaction = reaction;
			PageManager = pageManager;
			_AppPolicyManager = appPolicyManager;


			IsReactionValid = Reaction.ObserveProperty(x => x.IsValid)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			ReactionWorkName = Reaction.ToReactivePropertyAsSynchronized(x => x.Name)
				.AddTo(_CompositeDisposable);

			WorkFolderEditVM = new WorkFolderEditViewModel(PageManager, Reaction);
			FilterEditVM = new FilterEditViewModel(PageManager, Reaction);
			ActionsEditVM = new ActionsEditViewModel(Reaction, PageManager, _AppPolicyManager);
			DestinationEditVM = new DestinationEditViewModel(PageManager, Reaction);


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
