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
using ReactiveFolderStyles.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Modules.InstantAction.ViewModels
{
	public class InstantActionPageViewModel : BindableBase, INavigationAware
	{
		public IAppPolicyManager AppPolicyManger { get; private set; }


		public IRegionNavigationService NavigationService;


		public InstantActionModel Model { get; private set; }

		public ReadOnlyReactiveCollection<string> TargetFiles { get; private set; }


		public ReactiveProperty<InstantActionStepViewModel> InstantActionVM { get; private set; }

		public InstantActionPageViewModel(IAppPolicyManager appPolicyManager)
		{
			AppPolicyManger = appPolicyManager;

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

			Model = new InstantActionModel(AppPolicyManger);

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

		internal void ChangeStemp(InstantActionStepViewModel step)
		{
			if (step == null)
			{
				throw new Exception();
			}

			InstantActionVM.Value = step;
		}

		internal void ShowFileSelectStep()
		{
			InstantActionVM.Value = new FileSelectInstantActionStepViewModel(this, Model);

		}

		internal void ShowActioinSelectStep()
		{
			InstantActionVM.Value = new ActionsSelectInstantActionStepViewModel(this, Model);

		}
		internal void ShowFinishingStep()
		{
			InstantActionVM.Value = new FinishingInstantActionStepViewModel(this, Model);

		}

	}


	abstract public class InstantActionStepViewModel : BindableBase
	{
		public InstantActionPageViewModel PageVM { get; private set; }
		public InstantActionModel InstantAction { get; private set; }

		public InstantActionStepViewModel(InstantActionPageViewModel pageVM, InstantActionModel instantAction)
		{
			PageVM = pageVM;
			InstantAction = instantAction;
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
						PageVM.ChangeStemp(PreviewStep);
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
						PageVM.ChangeStemp(NextStep);
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
			Files = instantAction.TargetFiles.ToReadOnlyReactiveCollection(x => x.FilePath);
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


		private DelegateCommand<string> _RemoveTargetCommand;
		public DelegateCommand<string> RemoveTargetCommand
		{
			get
			{
				return _RemoveTargetCommand
					?? (_RemoveTargetCommand = new DelegateCommand<string>((filePath) =>
					{
						var removeTarget = InstantAction.TargetFiles.Single(x => x.FilePath == filePath);

						InstantAction.TargetFiles.Remove(removeTarget);

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

			// Actions
			// SelectedAppOptionsの追加に合わせてActionsを変更
			AppOptions.ObserveElementProperty(x => x.IsSelected, isPushCurrentValueAtFirst:false)
				.Select(pack => pack.Instance)
				.Subscribe(ToggleAction); 


			ActionVMs = new ObservableCollection<InstantAppOptionInstanceViewModel>(
				instantAction.Actions.Select(x => new InstantAppOptionInstanceViewModel(this, x))
				);
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

				InstantAction.Actions.Add(appOption._CachedActionVM.AppLaunchAction);

				ActionVMs.Add(appOption._CachedActionVM);
			}
			else
			{
				InstantAction.Actions.Remove(appOption._CachedActionVM.AppLaunchAction);
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
		public FinishingInstantActionStepViewModel(InstantActionPageViewModel pageVM, InstantActionModel instantAction)
			: base(pageVM, instantAction)
		{
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
	}
}
