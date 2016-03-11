using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.History;
using ReactiveFolder.Models.Util;
using ReactiveFolderStyles.Models;
using ReactiveFolderStyles.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Reactive.Bindings.Extensions;

namespace Modules.History.ViewModels
{
	public class HistoryPageViewModel : PageViewModelBase
	{
		public const int AdditionalLoadFilesAmount = 10;


		public IHistoryManager HistoryManager { get; private set; }
		public IFolderReactionMonitorModel Monitor { get; private set; }
		public IInstantActionManager InstantActionManager { get; private set; }

		public ObservableCollection<HistoryDataViewModel> ShowHistoryVMs { get; private set; }

		public List<FileInfo> HistoryFileInfoList { get; private set; }


		public ReactiveProperty<bool> CanIncrementalLoad { get; private set; }

		public HistoryPageViewModel(IHistoryManager histroyManager, PageManager pageManager, IFolderReactionMonitorModel monitor, IInstantActionManager instantActionManager)
			: base(pageManager)
		{
			HistoryManager = histroyManager;
			Monitor = monitor;
			InstantActionManager = instantActionManager;

			ShowHistoryVMs = new ObservableCollection<HistoryDataViewModel>();

			CanIncrementalLoad = ShowHistoryVMs.CollectionChangedAsObservable()
				.Select(_ => HistoryFileInfoList.Count > ShowHistoryVMs.Count)
				.ToReactiveProperty();

			IncrementalLoadHistoryCommand = CanIncrementalLoad
				.ToReactiveCommand();

			IncrementalLoadHistoryCommand.Subscribe(_ => IncrementalLoadHistoryItems());
		}


		public ReactiveCommand IncrementalLoadHistoryCommand { get; private set; }

		



		private void IncrementalLoadHistoryItems()
		{
			var startIndex = ShowHistoryVMs.Count;
			var endIndex = startIndex + AdditionalLoadFilesAmount;

			if (endIndex >= HistoryFileInfoList.Count)
			{
				endIndex = startIndex + (HistoryFileInfoList.Count - startIndex);
			}

			if (endIndex < 0)
			{
				return;
			}

			var loadCount = endIndex - startIndex;

			var files = HistoryFileInfoList.GetRange(startIndex, loadCount);

			var additionalHistoryVMs = files.Select(x => HistoryManager.LoadHistoryData(x))
				.Select(x => new HistoryDataViewModel(this, PageManager, Monitor, InstantActionManager, x));

			ShowHistoryVMs.AddRange(additionalHistoryVMs);
		}

		public static NavigationParameters CreateAppPolicyFilteringParameter(Guid appPolicyGuid)
		{
			var parameters = new NavigationParameters();

			parameters.Add("app_policy_guid", appPolicyGuid);

			return parameters;
		}

		public static NavigationParameters CreateReactionFilteringParameter(Guid reactionGuid)
		{
			var parameters = new NavigationParameters();

			parameters.Add("reaction_guid", reactionGuid);

			return parameters;
		}


		
		public override void OnNavigatedFrom(NavigationContext navigationContext)
		{
			ShowHistoryVMs.Clear();
		}

		public override void OnNavigatedTo(NavigationContext navigationContext)
		{
			HistoryFileInfoList = HistoryManager.GetHistoryDataFileList();

			IncrementalLoadHistoryItems();
		}
	}

	public class HistoryDataViewModel : BindableBase
	{
		public HistoryPageViewModel PageVM { get; private set; }
		public PageManager PageManager { get; private set; }
		public IFolderReactionMonitorModel Monitor { get; private set; }
		public IInstantActionManager InstantActionManage { get; private set; }

		public HistoryData HistoryData { get; private set; }

		public List<HistoryActionViewModel> Actions { get; private set; }
		public List<HistoryDataByFileViewModel> Files { get; private set; }

		public int TargetFileAmount { get; private set; }
		public int SuccessCount { get; private set; }

		public bool IsSourceReaction { get; private set; }
		public bool IsSourceInstantAction { get; private set; }

		public HistoryDataViewModel(HistoryPageViewModel pageVM, PageManager pageManager, IFolderReactionMonitorModel monitor, IInstantActionManager instantActionManager, HistoryData historyData)
		{
			PageVM = pageVM;
			PageManager = pageManager;
			Monitor = monitor;
			InstantActionManage = instantActionManager;
			HistoryData = historyData;

			Actions = HistoryData.Actions
				.Select(x => new HistoryActionViewModel(this, x))
				.ToList();

			Files = HistoryData.FileHistories
				.Select(x => new HistoryDataByFileViewModel(this, x))
				.ToList();

			TargetFileAmount = Files.Count;

			SuccessCount = Files.Where(x => x.IsSuccessed).Count();

			var fileName = Path.GetFileName(HistoryData.ActionSourceFilePath);
			if (fileName.EndsWith(FolderModel.REACTION_EXTENTION))
			{
				IsSourceReaction = true;
			}
			else if (fileName.EndsWith(".rfinstant.json"))
			{
				IsSourceInstantAction = true;
			}
			
		}




		private DelegateCommand _OpenActionSourceCommand;
		public DelegateCommand OpenActionSourceCommand
		{
			get
			{
				return _OpenActionSourceCommand
					?? (_OpenActionSourceCommand = new DelegateCommand(() =>
					{
						if (IsSourceReaction)
						{
							PageManager.OpenReaction(HistoryData.ActionSourceFilePath);
						}
						else if (IsSourceInstantAction)
						{
							PageManager.OpenInstantActionWithInstantActionFile(HistoryData.ActionSourceFilePath);
						}
					}
					, () => false == String.IsNullOrEmpty(HistoryData.ActionSourceFilePath) && 
							File.Exists(HistoryData.ActionSourceFilePath)
					));
			}
		}
	}



	public class HistoryActionViewModel : BindableBase
	{
		public HistoryDataViewModel ParentVM { get; private set; }
		public AppLaunchReactiveAction Action { get; private set; }

		public string AppName { get; private set; }
		
		public List<AppOptionInstanceViewModel> OptionInstances { get; private set; }

		public string OptionsText { get; private set; }

		public HistoryActionViewModel(HistoryDataViewModel parentVM, AppLaunchReactiveAction action)
		{
			ParentVM = parentVM;
			Action = action;

			AppName = action.AppPolicy?.AppName ?? "<Deleted AppPolicy>";
			OptionsText = String.Join("+", action.Options.Select(x => x.OptionDeclaration?.Name ?? ""));

			OptionInstances = action.Options
				.Select(x => new AppOptionInstanceViewModel(Action, x))
				.ToList();
		}
	}

	public class HistoryDataByFileViewModel : BindableBase
	{
		public HistoryDataViewModel ParentVM { get; private set; }
		public HistoryDataByFile FileHistory { get; private set; }


		public string InputFileName { get; private set; }
		public string OutputFileName { get; private set; }

		public string InputFilePath { get; private set; }
		public string OutputFilePath { get; private set; }

		public bool IsAlreadyOutputFile { get; private set; }

		public string StartTime { get; private set; }
		public string ProcessTime { get; private set; }

		public bool IsSuccessed { get; private set; }
		public bool IsFailed { get; private set; }

		public HistoryDataByFileViewModel(HistoryDataViewModel parentVM, HistoryDataByFile fileHistory)
		{
			ParentVM = parentVM;
			FileHistory = fileHistory;

			InputFileName = Path.GetFileName(FileHistory.InputFilePath);
			InputFilePath = FileHistory.InputFilePath;

			if (FileHistory.OutputFilePath != null)
			{
				OutputFileName = Path.GetFileName(FileHistory.OutputFilePath);
				OutputFilePath = FileHistory.OutputFilePath;
			}
			else
			{
				OutputFileName = "<No Output>";
				OutputFilePath = null; 
			}

			IsAlreadyOutputFile = FileHistory.OutputFilePath == null && FileHistory.IsSuccessed;

			StartTime = FileHistory.StartTime.ToShortTimeString();
			var totalSec = FileHistory.EndTime.Subtract(FileHistory.StartTime).TotalSeconds;

			ProcessTime = $"{totalSec:f3}";

			IsSuccessed = FileHistory.IsSuccessed;
			IsFailed = !IsSuccessed;

		}
	}
}
