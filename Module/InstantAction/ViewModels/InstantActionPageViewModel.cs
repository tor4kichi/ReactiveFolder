using Microsoft.Win32;
using Modules.InstantAction.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.AppPolicy;
using ReactiveFolderStyles.Models;
using ReactiveFolderStyles.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Modules.InstantAction.ViewModels
{
	public class InstantActionPageViewModel : BindableBase, INavigationAware, IDisposable
	{
		public IAppPolicyManager AppPolicyManger { get; private set; }
		public IInstantActionManager InstantActionManager { get; private set; }

		public IRegionNavigationService NavigationService;


		public InstantActionModel Model { get; private set; }

		public ReadOnlyReactiveCollection<string> TargetFiles { get; private set; }


		public ReactiveProperty<InstantActionStepViewModel> InstantActionVM { get; private set; }

		public InstantActionPageViewModel(IAppPolicyManager appPolicyManager, IInstantActionManager instantActionManager)
		{
			AppPolicyManger = appPolicyManager;
			InstantActionManager = instantActionManager;

			InstantActionVM = new ReactiveProperty<InstantActionStepViewModel>();
				
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

			if (Model == null)
			{
				Model = new InstantActionModel(AppPolicyManger);
				Model.OutputFolderPath = InstantActionManager.TempSaveFolder;
			}

			try
			{
				var paths = (string[])navigationContext.Parameters["filepaths"];

				if (paths != null)
				{
					foreach(var path in paths)
					{
						Model.AddTargetFile(path);
					}
				}
			}
			catch
			{
				
			}


			if (Model.TargetFiles.Count == 0)
			{
				// ファイル選択画面からスタート
				ShowFileSelectStep();
			}
			else
			{
				// 外部からファイルが提供されている場合にはアクション選択画面からスタート
				ShowActioinSelectStep();
			}
		}

		public static NavigationParameters MakeNavigationParamWithTargetFile(string[] paths)
		{
			var param = new NavigationParameters();
			param.Add("filepaths", paths);

			return param;
		}

		internal void ChangeStep(InstantActionStepViewModel step)
		{
			if (step == null)
			{
				throw new Exception();
			}

			if (InstantActionVM.Value != step)
			{
				InstantActionVM.Value?.Dispose();

				InstantActionVM.Value = step;
			}
		}

		internal void ShowFileSelectStep()
		{
			ChangeStep(new FileSelectInstantActionStepViewModel(this, Model));
		}

		internal void ShowActioinSelectStep()
		{
			ChangeStep(new ActionsSelectInstantActionStepViewModel(this, Model));

		}
		internal void ShowFinishingStep()
		{
			ChangeStep(new FinishingInstantActionStepViewModel(this, Model));
		}

		public void Dispose()
		{
			InstantActionVM.Value?.Dispose();
		}
	}


	abstract public class InstantActionStepViewModel : BindableBase, IDisposable
	{
		protected CompositeDisposable _CompositeDisposable;

		public InstantActionPageViewModel PageVM { get; private set; }
		public InstantActionModel InstantAction { get; private set; }

		public InstantActionStepViewModel(InstantActionPageViewModel pageVM, InstantActionModel instantAction)
		{
			PageVM = pageVM;
			InstantAction = instantAction;

			_CompositeDisposable = new CompositeDisposable();
		}

		private InstantActionStepViewModel _PreviousStep;
		public InstantActionStepViewModel PreviewStep
		{
			get
			{
				return _PreviousStep
					?? (_PreviousStep = CanGoPreview ? GetPreviewStep() : null);
			}
		}

		abstract public bool CanGoPreview { get; }
		virtual protected InstantActionStepViewModel GetPreviewStep() { return null; }

		private InstantActionStepViewModel _NextStep;
		public InstantActionStepViewModel NextStep
		{
			get
			{
				return _NextStep
					?? (_NextStep = CanGoNext ? GetNextStep() : null);
			}
		}

		abstract public bool CanGoNext { get; }
		virtual protected InstantActionStepViewModel GetNextStep() { return null; }

		private DelegateCommand _GoBackCommand;
		public DelegateCommand GoBackCommand
		{
			get
			{
				return _GoBackCommand
					?? (_GoBackCommand = new DelegateCommand(() =>
					{
						PageVM.ChangeStep(PreviewStep);
					},
						() => CanGoPreview					
					));
			}
		}
		private DelegateCommand _GoNextCommand;
		public DelegateCommand GoNextCommand
		{
			get
			{
				return _GoNextCommand
					?? (_GoNextCommand = new DelegateCommand(() =>
					{
						PageVM.ChangeStep(NextStep);
					},
						() => CanGoNext
					));
			}
		}



		protected void RaiseCanGoBackChanged()
		{
			GoBackCommand.RaiseCanExecuteChanged();
		}


		protected void RaiseCanGoNextChanged()
		{
			GoNextCommand.RaiseCanExecuteChanged();
		}

		public void Dispose()
		{
			_CompositeDisposable.Dispose();
		}

		private DelegateCommand<string[]> _FileDropedCommand;
		public DelegateCommand<string[]> FileDropedCommand
		{
			get
			{
				return _FileDropedCommand
					?? (_FileDropedCommand = new DelegateCommand<string[]>((filePaths) =>
					{
						foreach (var filePath in filePaths)
						{
							InstantAction.AddTargetFile(filePath);
						}

						RaiseCanGoNextChanged();
					}));
			}
		}

	}

	public class FileSelectInstantActionStepViewModel : InstantActionStepViewModel
	{
		// ファイルのD&D受付
		// ファイル選択ダイアログによるオープン
		// オプション）フォルダの内容を表示して、対象ファイルを選択

		public ReadOnlyReactiveCollection<string> Files { get; private set; }

		public FileSelectInstantActionStepViewModel(InstantActionPageViewModel pageVM, InstantActionModel instantAction)
			: base(pageVM, instantAction)
		{
			Files = instantAction.TargetFiles.ToReadOnlyReactiveCollection(x => x.FilePath)
				.AddTo(_CompositeDisposable);
		}




		private DelegateCommand _SelectTargetFileCommand;
		public DelegateCommand SelectTargetFileCommand
		{
			get
			{
				return _SelectTargetFileCommand
					?? (_SelectTargetFileCommand = new DelegateCommand(() => 
					{
						// TODO  
						var dialog = new OpenFileDialog();

						// downloadフォルダを初期設定にしてダイアログを開く
						dialog.Title = "Select you want process files";
						dialog.InitialDirectory = ReactiveFolderStyles.Util.KnownFolders.GetPath(ReactiveFolderStyles.Util.KnownFolder.Downloads);
						dialog.Multiselect = true;
						

						var result = dialog.ShowDialog();
						if (result.HasValue && true == result.Value)
						{
							var files = dialog.FileNames;

							foreach (var filePath in dialog.FileNames)
							{
								InstantAction.AddTargetFile(filePath);
							}
						}

						RaiseCanGoNextChanged();
					}));
			}
		}

		


		private DelegateCommand<string> _RemoveTargetCommand;
		public DelegateCommand<string> RemoveTargetCommand
		{
			get
			{
				return _RemoveTargetCommand
					?? (_RemoveTargetCommand = new DelegateCommand<string>((filePath) =>
					{
						var removeTarget = InstantAction.TargetFiles.Single(x => x.FilePath == filePath);

						InstantAction.RemoveTargetFile(removeTarget);

						RaiseCanGoNextChanged();
					}));
			}
		}

		public override bool CanGoPreview
		{
			get { return false; }
		}


		public override bool CanGoNext
		{
			get { return InstantAction.TargetFiles.Count > 0; }
		}

		protected override InstantActionStepViewModel GetNextStep()
		{
			return new ActionsSelectInstantActionStepViewModel(PageVM, InstantAction);
		}
	}

	public class ActionsSelectInstantActionStepViewModel : InstantActionStepViewModel
	{
		
		public ObservableCollection<ToggleSelectableInstantAppOptionViewModel> AppOptions { get; private set; }

		public ObservableCollection<InstantAppOptionInstanceViewModel> ActionVMs { get; private set; }

		public ActionsSelectInstantActionStepViewModel(InstantActionPageViewModel pageVM, InstantActionModel instantAction)
			: base(pageVM, instantAction)
		{
			AppOptions = new ObservableCollection<ToggleSelectableInstantAppOptionViewModel>(
				InstantAction.GenerateAppOptions()
				.Select(x => new ToggleSelectableInstantAppOptionViewModel(x))
				);

			ActionVMs = new ObservableCollection<InstantAppOptionInstanceViewModel>(
				instantAction.Actions.Select(x => new InstantAppOptionInstanceViewModel(this, x))
				);

			
				

			foreach (var appOption in AppOptions)
			{
				var vm = ActionVMs.SingleOrDefault(x => x.AppGuid == appOption.AppPolicy.Guid && x.OptionInstance.OptionDeclaration == appOption.Declaration);
				if (vm != null)
				{
					appOption.IsSelected = true;
					appOption._CachedActionVM = vm;
				}
			}

			// Actions
			// SelectedAppOptionsの追加に合わせてActionsを変更
			AppOptions.ObserveElementProperty(x => x.IsSelected, isPushCurrentValueAtFirst:false)
				.Select(pack => pack.Instance)
				.Subscribe(ToggleAction) 
				.AddTo(_CompositeDisposable);


		}

		// Note: 利用したいオプションとして選択用オプションとしては表示をOFFに
		private void ToggleAction(ToggleSelectableInstantAppOptionViewModel appOption)
		{
			if (appOption.IsSelected)
			{
				// Actionsに追加
				if (appOption._CachedActionVM == null)
				{
					var action = InstantActionModel.CreateOneOptionAppLaunchAction(appOption.AppPolicy.Guid, appOption.Declaration);
					appOption._CachedActionVM = new InstantAppOptionInstanceViewModel(this, action);
				}

				InstantAction.AddAction(appOption._CachedActionVM.AppLaunchAction);

				ActionVMs.Add(appOption._CachedActionVM);
			}
			else
			{
				InstantAction.RemoveAction(appOption._CachedActionVM.AppLaunchAction);
				ActionVMs.Remove(appOption._CachedActionVM);
			}

			RaiseCanGoNextChanged();
		}



		public override bool CanGoPreview
		{
			get { return true; }
		}


		public override bool CanGoNext
		{
			get { return InstantAction.Actions.Count > 0; }
		}

		protected override InstantActionStepViewModel GetPreviewStep()
		{
			return new FileSelectInstantActionStepViewModel(PageVM, InstantAction);
		}

		protected override InstantActionStepViewModel GetNextStep()
		{
			return new FinishingInstantActionStepViewModel(PageVM, InstantAction);
		}

		internal void RemoveAction(AppLaunchReactiveAction action)
		{
			var appOption = AppOptions.SingleOrDefault(x =>
				x.AppPolicy.Guid == action.AppGuid &&
				x.Declaration.Id == action.AdditionalOptions[0].OptionId);

			if (appOption != null)
			{
				appOption.IsSelected = false;
			}
			else
			{
				var vm = ActionVMs.Single(x => x.AppLaunchAction == action);
				ActionVMs.Remove(vm);
				InstantAction.RemoveAction(action);
			}
		}
	}


	public class ToggleSelectableInstantAppOptionViewModel : BindableBase
	{
		public InstantAppOptionInstanceViewModel _CachedActionVM { get; set; }

		public InstantAppOption AppOption { get; private set; }

		public ApplicationPolicy AppPolicy
		{
			get
			{
				return AppOption.AppPolicy;
			}
		}

		public AppOptionDeclarationBase Declaration
		{
			get
			{
				return AppOption.Declaration;
			}
		}

		private bool _IsSelected;
		public bool IsSelected
		{
			get
			{
				return _IsSelected;
			}
			set
			{
				SetProperty(ref _IsSelected, value);
			}
		}


		public string AppName { get; private set; }

		public string OptionName { get; private set; }
		

		public ToggleSelectableInstantAppOptionViewModel(InstantAppOption appOption)
		{
			AppOption = appOption;
			AppName = AppPolicy.AppName;
			OptionName = Declaration.Name;

		}

		private DelegateCommand _ToggleSelectCommand;
		public DelegateCommand ToggleSelectCommand
		{
			get
			{
				return _ToggleSelectCommand
					?? (_ToggleSelectCommand = new DelegateCommand(() =>
					{
						IsSelected = !IsSelected;   // toggle selected
					}));
			}
		}

		
	}


	public class InstantAppOptionInstanceViewModel : BindableBase
	{
		public ActionsSelectInstantActionStepViewModel ActionSelectStepVM { get; private set; }

		public AppLaunchReactiveAction AppLaunchAction { get; private set; }
		public AppOptionInstance OptionInstance { get; private set; }

		public List<AppOptionValueViewModel> OptionValues { get; private set; }


		public string AppName { get; private set; }
		public Guid AppGuid { get; private set; } 

		public string OptionName { get; private set; }

		public InstantAppOptionInstanceViewModel(ActionsSelectInstantActionStepViewModel parentVM, AppLaunchReactiveAction action)
		{
			ActionSelectStepVM = parentVM;

			AppLaunchAction = action;
			OptionInstance = action.AdditionalOptions[0];
			var appPolicy = action.AppPolicy;


			AppName = appPolicy.AppName;
			AppGuid = appPolicy.Guid;

			OptionName = OptionInstance.OptionDeclaration.Name;

			OptionValues = OptionInstance.FromAppOptionInstance()
				.ToList();

			
		}

		private DelegateCommand _RemoveActionCommand;
		public DelegateCommand RemoveActionCommand
		{
			get
			{
				return _RemoveActionCommand
					?? (_RemoveActionCommand = new DelegateCommand(() =>
					{
						ActionSelectStepVM.RemoveAction(AppLaunchAction);
					}));
			}
		}

		
	}









	public class FinishingInstantActionStepViewModel : InstantActionStepViewModel
	{
		public ReadOnlyReactiveCollection<ProcessingFileViewModel> Files { get; private set; }

		public ReactiveProperty<string> OutputFolderPath { get; private set; }

		public ReactiveProperty<int> FileCount { get; private set; }
		public ReactiveProperty<int> ProcessedCount { get; private set; }
		public ReactiveProperty<int> FailedCount { get; private set; }



		public ReactiveProperty<bool> IsAllChecked { get; private set; }


		/// <summary>
		/// VM内からIsAllCheckedを操作した時に処理をスキップするためのフラグ
		/// </summary>
		private bool NowWorkingFileSelection = false;



		private object _InstantActionProcessLock = new object();





		public FinishingInstantActionStepViewModel(InstantActionPageViewModel pageVM, InstantActionModel instantAction)
			: base(pageVM, instantAction)
		{
			Files = InstantAction.TargetFiles.ToReadOnlyReactiveCollection(x =>
				new ProcessingFileViewModel(InstantAction, x)
				)
				.AddTo(_CompositeDisposable);

			OutputFolderPath = InstantAction.ObserveProperty(x => x.OutputFolderPath)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			FileCount = InstantAction.TargetFiles.CollectionChangedAsObservable().ToUnit()
				.Select(_ => InstantAction.TargetFiles.Count)
				.ToReactiveProperty(InstantAction.TargetFiles.Count)
				.AddTo(_CompositeDisposable);

			ProcessedCount = Observable.Merge(
				InstantAction.TargetFiles.CollectionChangedAsObservable().ToUnit(),
				InstantAction.TargetFiles.ObserveElementProperty(x => x.ProcessState).ToUnit()
				)
				.Select(_ =>
				{
					return InstantAction.TargetFiles.Where(x => x.IsComplete).Count();
				})
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			FailedCount = ProcessedCount.Select(x => FileCount.Value - x)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

			IsAllChecked = new ReactiveProperty<bool>(false)
				.AddTo(_CompositeDisposable);

			IsAllChecked.Subscribe(_ => 
			{
				if (NowWorkingFileSelection) { return; }

				var allCheck = ! Files.Any(x => x.IsSelected == true);

				foreach (var file in Files)
				{
					file.IsSelected = allCheck;
				}
			})
				.AddTo(_CompositeDisposable);

			Files.ObserveElementProperty(x => x.IsSelected)
				.Subscribe(_ => 
				{
					NowWorkingFileSelection = true;

					IsAllChecked.Value = Files.Any(x => x.IsSelected == true);

					NowWorkingFileSelection = false;
				})
				.AddTo(_CompositeDisposable);

			InstantAction.TargetFiles.ObserveAddChanged()
				.Subscribe(x => 
				{
					Task.Run(() =>
					{
						lock (_InstantActionProcessLock)
						{
							if (x.IsReady)
							{
								InstantAction.Execute(x);
							}
						}
					});
				})
				.AddTo(_CompositeDisposable);

			StartProcess();
		}



		public override bool CanGoPreview
		{
			get { return true; }
		}


		public override bool CanGoNext
		{
			get { return false; }
		}

		protected override InstantActionStepViewModel GetPreviewStep()
		{
			return new ActionsSelectInstantActionStepViewModel(PageVM, InstantAction);
		}


		private DelegateCommand _ChangeSaveFolderCommand;
		public DelegateCommand ChangeSaveFolderCommand
		{
			get
			{
				return _ChangeSaveFolderCommand
					?? (_ChangeSaveFolderCommand = new DelegateCommand(() =>
					{
						var folderDialog = new WPFFolderBrowser.WPFFolderBrowserDialog();
						folderDialog.Title = "";
						folderDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

						var result = folderDialog.ShowDialog();
						if (result.HasValue && result.Value == true)
						{
							InstantAction.OutputFolderPath = folderDialog.FileName;
						}

					}));
			}
		}

		


		private DelegateCommand _AllFilePathCopyToClipboardCommand;
		public DelegateCommand AllFilePathCopyToClipboardCommand
		{
			get
			{
				return _AllFilePathCopyToClipboardCommand
					?? (_AllFilePathCopyToClipboardCommand = new DelegateCommand(() =>
					{
						System.Collections.Specialized.StringCollection files =
							new System.Collections.Specialized.StringCollection();
						foreach (var fileVM in Files)
						{
							files.Add(fileVM.TargetFile.OutputPath);
						}

						Clipboard.SetFileDropList(files);

					}));
			}
		}

		private DelegateCommand _SelectedFilePathCopyToClipboardCommand;
		public DelegateCommand SelectedFilePathCopyToClipboardCommand
		{
			get
			{
				return _SelectedFilePathCopyToClipboardCommand
					?? (_SelectedFilePathCopyToClipboardCommand = new DelegateCommand(() =>
					{
						System.Collections.Specialized.StringCollection files =
							new System.Collections.Specialized.StringCollection();
						foreach (var fileVM in Files.Where(x => x.IsSelected))
						{
							files.Add(fileVM.TargetFile.OutputPath);
						}

						Clipboard.SetFileDropList(files);
							
					}));
			}
		}


		private DelegateCommand _DeleteSelectedCommand;
		public DelegateCommand DeleteSelectedCommand
		{
			get
			{
				return _DeleteSelectedCommand
					?? (_DeleteSelectedCommand = new DelegateCommand(() =>
					{
						var selectedFiles = Files.Where(x => x.IsSelected)
							.Select(x => x.TargetFile);

						foreach (var targetFile in selectedFiles)
						{
							InstantAction.RemoveTargetFile(targetFile);
						}
					}));
			}
		}

		private DelegateCommand _OpenWithDefaultAppCommand;
		public DelegateCommand OpenWithDefaultAppCommand
		{
			get
			{
				return _OpenWithDefaultAppCommand
					?? (_OpenWithDefaultAppCommand = new DelegateCommand(() =>
					{
						var selectedFiles = Files.Where(x => x.IsSelected)
							.Select(x => x.TargetFile);

						foreach (var targetFile in selectedFiles)
						{
							if (targetFile.IsComplete)
							{
								Process.Start(targetFile.OutputPath);
							}
						}
					}));
			}
		}


		private DelegateCommand _CreateReactionCommand;
		public DelegateCommand CreateReactionCommand
		{
			get
			{
				return _CreateReactionCommand
					?? (_CreateReactionCommand = new DelegateCommand(() =>
					{

					}));
			}
		}

		private DelegateCommand _DoneCommand;
		public DelegateCommand DoneCommand
		{
			get
			{
				return _DoneCommand
					?? (_DoneCommand = new DelegateCommand(() =>
					{

					}));
			}
		}

		
		public void StartProcess()
		{
			// TODO: 
			Task.Run(() =>
			{
				foreach (var file in InstantAction.TargetFiles.Where(x => x.IsReady))
				{
					lock (_InstantActionProcessLock)
					{
						InstantAction.Execute(file);
					}
				}
			});
		}
		
	}


	public class ProcessingFileViewModel : BindableBase, IDisposable
	{
		public InstantActionModel InstantAction { get; private set; }
		public InstantActionTargetFile TargetFile { get; private set; }

		private bool _IsSelected;
		public bool IsSelected
		{
			get
			{
				return _IsSelected;
			}
			set
			{
				SetProperty(ref _IsSelected, value);
			}
		}

		public ReactiveProperty<FileProcessState> ProcessState { get; private set; }

		public string FilePath { get; private set; }

		public ReactiveProperty<string> Message { get; private set; }

		public ProcessingFileViewModel(InstantActionModel instantAction, InstantActionTargetFile targetFile)
		{
			InstantAction = instantAction;
			TargetFile = targetFile;

			FilePath = targetFile.FilePath;
			ProcessState = targetFile.ObserveProperty(x => x.ProcessState)
				.ToReactiveProperty();

			Message = TargetFile.ObserveProperty(x => x.ProcessMessage)
				.ToReactiveProperty();
		}

		public void Dispose()
		{
			ProcessState.Dispose();
			Message.Dispose();
		}
	}
}
