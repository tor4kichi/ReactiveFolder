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

namespace Modules.Main.ViewModels
{

	public enum ReactionFilterType
	{
		Files,
		Folder,

		Unknown
	}

	public class ReactionEditerPageViewModel : PageViewModelBase, INavigationAware
	{
		private FolderReactionModel Reaction;

		public IRegionNavigationService NavigationService;

		public ReactiveProperty<bool> IsReactionValid { get; private set; }

		public ReactiveProperty<string> ReactionWorkName { get; private set; }
		public ReactiveProperty<string> WorkFolderPath { get; private set; }


		public ReactiveProperty<FilterEditViewModel> FilterEditVM { get; private set; }
		public ReactiveProperty<TimingEditViewModel> TimingEditVM { get; private set; }
		public ReactiveProperty<ActionsEditViewModel> ActionsEditVM { get; private set; }
		public ReactiveProperty<DestinationEditViewModel> DestinationEditVM { get; private set; }

		public ReactiveProperty<bool> IsEnableSave { get; private set; }


		public ReactionEditerPageViewModel(IRegionManager regionManager, IRegionNavigationService navService, FolderReactionMonitorModel monitor)
			: base(regionManager, monitor)
		{
			IsReactionValid = new ReactiveProperty<bool>(false);

			ReactionWorkName = new ReactiveProperty<string>("");
			WorkFolderPath = new ReactiveProperty<string>("");

			FilterEditVM = new ReactiveProperty<FilterEditViewModel>();
			TimingEditVM = new ReactiveProperty<TimingEditViewModel>();
			ActionsEditVM = new ReactiveProperty<ActionsEditViewModel>();
			DestinationEditVM = new ReactiveProperty<DestinationEditViewModel>();

			IsEnableSave = new ReactiveProperty<bool>(true);
		}


		private void Initialize()
		{
			// initialize with this.Reaction

			ReactionWorkName.Value = Reaction.Name;
			WorkFolderPath.Value = Reaction.WorkFolder.FullName;

			Reaction.ObserveProperty(x => x.WorkFolder)
				.Subscribe(x =>
				{
					FilterEditVM.Value?.Dispose();

					FilterEditVM.Value = new FilterEditViewModel(Reaction);
					TimingEditVM.Value = new TimingEditViewModel(Reaction);
					ActionsEditVM.Value = new ActionsEditViewModel(Reaction);
					DestinationEditVM.Value = new DestinationEditViewModel(Reaction);
				});

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
						// TODO:
					}));
			}
		}



		private DelegateCommand _FolderSelectCommand;
		public DelegateCommand FolderSelectCommand
		{
			get
			{
				return _FolderSelectCommand
					?? (_FolderSelectCommand = new DelegateCommand(() =>
					{
						OpenFileDialog ofp = new OpenFileDialog();
						ofp.FileName = "The file will be ignored";
						ofp.CheckFileExists = false;
						ofp.CheckPathExists = true;
						ofp.ValidateNames = false;

						try
						{
							var result = ofp.ShowDialog();
							if (result == DialogResult.OK &&
								false == String.IsNullOrWhiteSpace(ofp.FileName))
							{
								var folderPath = Path.GetDirectoryName(ofp.FileName);
								var folderInfo = new DirectoryInfo(folderPath);

								if (false == folderInfo.Exists)
								{
									return;
								}
								Reaction.WorkFolder = folderInfo;
								WorkFolderPath.Value = folderInfo.FullName;
							}
						}
						finally
						{
							ofp.Dispose();
						}
						//						var dialog = new FolderBrowserDialog();

					}));
			}
		}


		
		
	}



	// FilesかFolderを判断してFiles用/Folder用のViewModelを生成する

	public class FilterEditViewModel : BindableBase , IDisposable
	{

		public FolderReactionModel Reaction { get; private set; }


		/// <summary>
		/// FileまたはFolderのフィルターモデルタイプ
		/// </summary>
		public ReactiveProperty<ReactionFilterType> FilterType { get; private set; }


		/// <summary>
		/// FileまたはFolderのフィルターViewModel
		/// FilterTypeに反応してVMを更新する
		/// </summary>
		public ReactiveProperty<FilterViewModelBase> FilterVM { get; private set; }


		// Note: IsFileFilterSelectedとIsFolderFilterSelectedはコマンドでよくね？
		// という意見もあるが、ラジオボタンを初期化する際にVM側からFileかFolderを伝えるために
		// わかりやすさを重視して二つとものフラグを持つ形にしている

		/// <summary>
		/// FileFilterを選択しているかのフラグ
		/// </summary>
		public ReactiveProperty<bool> IsFileFilterSelected { get; private set; }

		/// <summary>
		/// FolderFilterを選択しているかのフラグ
		/// </summary>
		public ReactiveProperty<bool> IsFolderFilterSelected { get; private set; }



		// TODO: FilterVMをキャッシュして使いまわせるようにしたい

		/// <summary>
		/// FilterVMをちゃんと消すために使うDisposer
		/// FilterVMを使いまわさずに切り替えの都度生成しているため必要
		/// </summary>
		private IDisposable _FilterVMDisposer;


		private CompositeDisposable _CompositeDisposable;


		/// <summary>
		/// FileModelのキャッシュ
		/// 切り替え後も前回入力した状態を保持する
		/// </summary>
		private FileReactiveFilter _CachedFileModel;


		/// <summary>
		/// FolderModelのキャッシュ
		/// 切り替え後も前回入力した状態を保持する
		/// </summary>
		private FolderReactiveFilter _CachedFolderModel;





		public FilterEditViewModel(FolderReactionModel reactionModel)
		{
			Reaction = reactionModel;

			_CompositeDisposable = new CompositeDisposable();


			// 現在のフィルターの状態からFile用VMかFolder用VMを選択
			var currentFilterType = FilterModelToVMType(Reaction.Filter);

			FilterType = new ReactiveProperty<ReactionFilterType>(currentFilterType)
				.AddTo(_CompositeDisposable);

			FilterVM = FilterType.Select(FilterTypeToFilterVM)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);


			// FilterEdit内でフィルタータイプ切り替えによって生成されるVMのDispose管理
			_FilterVMDisposer = null;
			FilterVM.Subscribe(x =>
			{
				_FilterVMDisposer?.Dispose();
				_FilterVMDisposer = x;
			})
			.AddTo(_CompositeDisposable);
			


			IsFileFilterSelected = new ReactiveProperty<bool>(currentFilterType == ReactionFilterType.Files)
				.AddTo(_CompositeDisposable);
			IsFileFilterSelected
				.Where(x => x)
				.Subscribe(_ => FilterType.Value = ReactionFilterType.Files)
				.AddTo(_CompositeDisposable);

			IsFolderFilterSelected = new ReactiveProperty<bool>(currentFilterType == ReactionFilterType.Folder)
				.AddTo(_CompositeDisposable);
			IsFolderFilterSelected
				.Where(x => x)
				.Subscribe(_ => FilterType.Value = ReactionFilterType.Folder)
				.AddTo(_CompositeDisposable);
		}


		public ReactionFilterType FilterModelToVMType(ReactiveFilterBase filter)
		{
			if (filter is FileReactiveFilter)
			{
				return ReactionFilterType.Files;
			}
			else if (filter is FolderReactiveFilter)
			{
				return ReactionFilterType.Folder;
			}
			else 
			{
				return ReactionFilterType.Unknown;
			}
		}

		FilterViewModelBase FilterTypeToFilterVM(ReactionFilterType filterType)
		{


			// FileとFolderのModelをそれぞれ一つだけ作成するようキャッシュを通して
			// Reaction.Filterを再設定する

			// その上でFileとFolderのFilterViewModelBaseを作成して返すようにする



			var currentType = FilterModelToVMType(Reaction.Filter);

			switch (currentType)
			{
				case ReactionFilterType.Files:
					_CachedFileModel = Reaction.Filter as FileReactiveFilter;
					break;
				case ReactionFilterType.Folder:
					_CachedFolderModel = Reaction.Filter as FolderReactiveFilter;
					break;
				case ReactionFilterType.Unknown:
					break;
				default:
					break;
			}


			switch (filterType)
			{
				case ReactionFilterType.Files:
					if (_CachedFileModel == null)
					{
						_CachedFileModel = new FileReactiveFilter();
					}
					Reaction.Filter = _CachedFileModel;
					return new FileFilterViewModel(Reaction);
				case ReactionFilterType.Folder:
					if (_CachedFolderModel == null)
					{
						_CachedFolderModel = new FolderReactiveFilter();
					}
					Reaction.Filter = _CachedFolderModel;
					return new FolderFilterViewModel(Reaction);
				case ReactionFilterType.Unknown:
					return null;
				default:
					throw new Exception("");
			}
		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;

			_FilterVMDisposer?.Dispose();
			_FilterVMDisposer = null;
		}
	}



	// FilterEditViewModelが管理する対象となるFileVMとFolderVMのベース


	public class FilterViewModelBase : BindableBase, IDisposable
	{
		public FolderReactionModel ReactionModel { get; private set; }

		protected CompositeDisposable _CompositeDisposable { get; private set; }


		public FilterViewModelBase(FolderReactionModel reactionModel)
		{
			ReactionModel = reactionModel;
			_CompositeDisposable = new CompositeDisposable();
		}



		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}

	}

	
	/// <summary>
	/// FileをフィルターするやつのViewModel
	/// ファイルを対象とする場合、フォルダと違って複数の拡張子に対応できるよう
	/// 複数のフィルターパターン文字列を受け取れるようになっている。
	/// </summary>
	public class FileFilterViewModel : FilterViewModelBase
	{
		public static readonly FileFilterViewModel Empty = new FileFilterViewModel();


		// 足りないもの
		// 追加ようテキストボックスのテキスト
		// Word追加コマンド
		// Word削除コマンド



		/// <summary>
		/// 現在追加されているFileのフィルターパターン
		/// Model -> ViewModelへの一方通行
		/// FilterParttersを変更する場合はFileReactiveFilterに追加削除を行う。
		/// </summary>
		public ReadOnlyReactiveCollection<string> FileFilterPatterns { get; private set; }

		/// <summary>
		/// 候補ワード
		/// FilterPartternsが変更される都度、更新される。
		/// </summary>
		public ObservableCollection<string> CandidateFilterItems { get; private set; }

		private ReadOnlyReactiveCollection<string> _CachedCandidateFilterItems;

		/// <summary>
		/// FilterPatternsの検証結果
		/// </summary>
		public ReactiveProperty<bool> IsValid { get; private set; }


		/// <summary>
		/// FilterPartternsをもとにFolderReactionModel.WorkFolder内の
		/// ファイルをフィルタリングした結果のファイル名（拡張子含む）
		/// </summary>
		public ObservableCollection<string> SampleItems { get; private set; }



		private FileReactiveFilter _FileFilterModel;

		public FileFilterViewModel()
			: base(null)
		{
			IsValid = new ReactiveProperty<bool>(false);

			var temp = new ObservableCollection<string>();
			FileFilterPatterns = temp.ToReadOnlyReactiveCollection();
			CandidateFilterItems = new ObservableCollection<string>();
			SampleItems = new ObservableCollection<string>();
		}


		public FileFilterViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			_FileFilterModel = ReactionModel.Filter as FileReactiveFilter;


			FileFilterPatterns = _FileFilterModel.FileFilterPatterns
				.ToReadOnlyReactiveCollection()
				.AddTo(_CompositeDisposable);

			CandidateFilterItems = new ObservableCollection<string>();

			// FileFilterPatternの候補ワード
			// 
			_CachedCandidateFilterItems = ReactionModel.ObserveProperty(x => x.WorkFolder)
				.Select(_ => ReactiveFilterHelper.GetFileCandidateFilterPatterns(ReactionModel))
				.Do(x =>
				{
					CandidateFilterItems.Clear();
					CandidateFilterItems.AddRange(x);
				})
				.SelectMany(x => x)
				.ToReadOnlyReactiveCollection()
				.AddTo(_CompositeDisposable);
				



			IsValid = FileFilterPatterns.PropertyChangedAsObservable()
				.Select(_ =>
				{
					if (_FileFilterModel.Validate().HasValidationError) { return false; }

					return true;
				})
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);



			SampleItems = new ObservableCollection<string>(_FileFilterModel.FileFilter(ReactionModel.WorkFolder).Select(x => x.Name));

			_FileFilterModel.FileFilterPatterns
				.CollectionChangedAsObservable()
				.Subscribe(_ =>
				{
					SampleItems.Clear();
					SampleItems.AddRange(
						_FileFilterModel.FileFilter(ReactionModel.WorkFolder).Select(x => x.Name)
						);
				})
				.AddTo(_CompositeDisposable);
		}



		// FiterText AddCommand

		private DelegateCommand<string> _AddFiterTextCommand;
		public DelegateCommand<string> AddFiterTextCommand
		{
			get
			{
				return _AddFiterTextCommand
					?? (_AddFiterTextCommand = new DelegateCommand<string>(word =>
					{
						_FileFilterModel.FileFilterPatterns.Add(word);

						if (CandidateFilterItems.Contains(word))
						{
							CandidateFilterItems.Remove(word);
						}
					}));
			}
		}



		private DelegateCommand<string> _RemoveFiterTextCommand;
		public DelegateCommand<string> RemoveFiterTextCommand
		{
			get
			{
				return _RemoveFiterTextCommand
					?? (_RemoveFiterTextCommand = new DelegateCommand<string>(word =>
					{
						_FileFilterModel.FileFilterPatterns.Remove(word);

						if (_CachedCandidateFilterItems.Contains(word))
						{
							CandidateFilterItems.Add(word);
						}
					}));
			}
		}

	}



	/// <summary>
	/// FilterEditViewModelに管理されるフォルダーフィルターモデル向けのVM
	/// フォルダは一つだけ選択できる。
	/// </summary>
	public class FolderFilterViewModel : FilterViewModelBase
	{
		public static readonly FolderFilterViewModel Empty = new FolderFilterViewModel();

		// Note: Folderは選択できるのは一つだけ


		/// <summary>
		/// フォルダのフィルターパターン
		/// </summary>
		/// <see cref="FolderReactiveFilter"/>
		public ReactiveProperty<string> FolderFilterPattern { get; private set; }


		/// <summary>
		/// FolderFilterPatternの検証結果
		/// </summary>
		public ReactiveProperty<bool> IsValid { get; private set; }

		/// <summary>
		/// FolderFilterPatternに自動入力できる候補ワード
		/// </summary>
		public ReadOnlyReactiveCollection<string> CandidateFilterItems { get; private set; }


		/// <summary>
		/// FolderFilterPatternをFolderReactiveModel.WorkFolderのフォルダに
		/// 実際に適用した結果のフォルダ名
		/// </summary>
		public ReadOnlyReactiveCollection<string> SampleItems { get; private set; }




		private FolderReactiveFilter _FolderFilter;


		public FolderFilterViewModel()
			: base(null)
		{
			IsValid = new ReactiveProperty<bool>(false);
			FolderFilterPattern = new ReactiveProperty<string>("");

			var temp = new ObservableCollection<string>();
			CandidateFilterItems = temp.ToReadOnlyReactiveCollection();
			SampleItems = temp.ToReadOnlyReactiveCollection();
		}


		public FolderFilterViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			_FolderFilter = ReactionModel.Filter as FolderReactiveFilter;

			FolderFilterPattern = _FolderFilter
				.ToReactivePropertyAsSynchronized(x => x.FolderFilterPattern, 
				convert: x => 
				{
					// Model -> VM
					return $"/{x}";
				},
				convertBack: x => 
				{
					// VM -> M
					if (x.StartsWith("/"))
					{
						x = x.Substring(1);
					}
					return x;
				}
				)
				.AddTo(_CompositeDisposable);

			IsValid = _FolderFilter.ObserveProperty(x => x.FolderFilterPattern)
				.Select(x =>
				{
					return false == _FolderFilter.Validate().HasValidationError;
				})
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);


			CandidateFilterItems = ReactionModel.ObserveProperty(x => x.WorkFolder)
				.SelectMany(x => ReactiveFilterHelper.GetFolderCandidateFilterPatterns(ReactionModel))
				.ToReadOnlyReactiveCollection()
				.AddTo(_CompositeDisposable);


			SampleItems = _FolderFilter.ObserveProperty(x => x.FolderFilterPattern)
				.Throttle(TimeSpan.FromSeconds(0.25))
				.SelectMany(x => _FolderFilter.DirectoryFilter(ReactionModel.WorkFolder))
				.Select(x => $"/{x.Name}")
				.ToReadOnlyReactiveCollection(FolderFilterPattern.ToUnit())
				.AddTo(_CompositeDisposable);
		}








		private DelegateCommand<string> _SelectCandidateWordCommand;
		public DelegateCommand<string> SelectCandidateWordCommand
		{
			get
			{
				return _SelectCandidateWordCommand
					?? (_SelectCandidateWordCommand = new DelegateCommand<string>(word =>
					{
						FolderFilterPattern.Value = word;
					}));
			}
		}

		
	}
	


	public class TimingEditViewModel : BindableBase, IDisposable
	{
		public FolderReactionModel ReactionModel;

		protected CompositeDisposable _CompositeDisposable { get; private set; }

		

		public List<TimingViewModelBase> TimingVMs { get; private set; }

		public TimingEditViewModel(FolderReactionModel reactionModel)
		{
			ReactionModel = reactionModel;
			_CompositeDisposable = new CompositeDisposable();


			TimingVMs = new List<TimingViewModelBase>();

			TimingVMs.Add(new FileUpdateTimingViewModel(ReactionModel));
			TimingVMs.Add(new TimerTimingViewModel(ReactionModel));
		}


		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}
	}


	// タイミングの種類ごとにVMが必要

	public class TimingViewModelBase : BindableBase, IDisposable
	{
		public FolderReactionModel ReactionModel { get; private set; }

		protected CompositeDisposable _CompositeDisposable { get; private set; }


		public TimingViewModelBase(FolderReactionModel reactionModel)
		{
			ReactionModel = reactionModel;
			_CompositeDisposable = new CompositeDisposable();
		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}
	}

	public class TimingViewModelTemplate<TIMING_MODEL> : TimingViewModelBase
		where TIMING_MODEL: ReactiveTimingBase, new()
	{
		
		public ReactiveProperty<bool> IsEnable { get; private set; }

		private TIMING_MODEL _CachedModel;


		public TimingViewModelTemplate(FolderReactionModel reactionModel)
			: base(reactionModel)
		{

			var timingModel = ReactionModel.Timings.SingleOrDefault(x => x is TIMING_MODEL);
			var hasTypedTimingModel = timingModel != null;
			IsEnable = new ReactiveProperty<bool>(hasTypedTimingModel);

			_CachedModel = timingModel as TIMING_MODEL; 

			IsEnable.Subscribe(x =>
			{
				if (x)
				{
					if (_CachedModel == null)
					{
						_CachedModel = new TIMING_MODEL();
					}
					ReactionModel.AddTiming(_CachedModel);
				}
				else
				{
					ReactionModel.RemoveTiming(_CachedModel);
				}
			})
			.AddTo(_CompositeDisposable);
		}



	}

	public class FileUpdateTimingViewModel : TimingViewModelTemplate<FileUpdateReactiveTiming>
	{
		public FileUpdateTimingViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			
		}
	}

	public class TimerTimingViewModel : TimingViewModelTemplate<TimerReactiveTiming>
	{
		public TimerTimingViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{

		}
	}

	public class ActionsEditViewModel : BindableBase
	{

		public FolderReactionModel ReactionModel;
		public ActionsEditViewModel(FolderReactionModel reactionModel)
		{
			ReactionModel = reactionModel;
		}
	}

	public class ActionViewModelBase : BindableBase, IDisposable
	{
		public FolderReactionModel ReactionModel { get; private set; }

		protected CompositeDisposable _CompositeDisposable { get; private set; }

		public ActionViewModelBase(FolderReactionModel reactionModel)
		{
			ReactionModel = reactionModel;
			_CompositeDisposable = new CompositeDisposable();
		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}
	}


	public class AppLaunchActionViewModel : ActionViewModelBase
	{
		public static List<string> AppList;

		static AppLaunchActionViewModel()
		{
			AppList = AppLaunchReactiveAction.AppPolicyFactory.GetPolicies()
				.Select(x => x.AppName)
				.ToList();
		}

		// アプリの選択とアプリ内のAppOptionの選択

		public AppLaunchActionViewModel(FolderReactionModel reactionModel)
			 : base(reactionModel)
		{

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

	}



	public class DestinationEditViewModel : BindableBase, IDisposable
	{
		public FolderReactionModel ReactionModel;

		protected CompositeDisposable _CompositeDisposable { get; private set; }

		public ReadOnlyReactiveProperty<DestinationViewModelBase> DestinationVM { get; private set; }

		public ReactiveProperty<bool> IsSameInputFolderChecked { get; set; }
		public ReactiveProperty<bool> IsInputChildFolderChecked { get; set; }
		public ReactiveProperty<bool> IsAbsoluteFolderChecked { get; set; }


		SameInputReactiveDestination _CachedSameInputDest;
		ChildReactiveDestination _CachedInputChildFolderDest;
		AbsolutePathReactiveDestination _CachedAbsoluteDest;

		public DestinationEditViewModel(FolderReactionModel reactionModel)
		{
			ReactionModel = reactionModel;
			_CompositeDisposable = new CompositeDisposable();

			CacheDestinationModel();

			// 1. Checked系が変化すると
			IsSameInputFolderChecked = new ReactiveProperty<bool>(ReactionModel.Destination is SameInputReactiveDestination)
				.AddTo(_CompositeDisposable); 
			IsInputChildFolderChecked = new ReactiveProperty<bool>(ReactionModel.Destination is ChildReactiveDestination)
				.AddTo(_CompositeDisposable);
			IsAbsoluteFolderChecked = new ReactiveProperty<bool>(ReactionModel.Destination is AbsolutePathReactiveDestination)
				.AddTo(_CompositeDisposable);


			// 2. CheckedのSubscriberによってReactionModel.Destinationが更新されて
			IsSameInputFolderChecked.Where(x => x)
				.Subscribe(_ =>
				{
					CacheDestinationModel();

					if (_CachedSameInputDest == null)
					{
						_CachedSameInputDest = new SameInputReactiveDestination();
					}

					ReactionModel.Destination = _CachedSameInputDest;
				})
				.AddTo(_CompositeDisposable);

			IsInputChildFolderChecked.Where(x => x)
				.Subscribe(_ =>
				{
					CacheDestinationModel();

					if (_CachedInputChildFolderDest == null)
					{
						_CachedInputChildFolderDest = new ChildReactiveDestination();
					}

					ReactionModel.Destination = _CachedInputChildFolderDest;
				})
				.AddTo(_CompositeDisposable);

			IsAbsoluteFolderChecked.Where(x => x)
				.Subscribe(_ =>
				{
					CacheDestinationModel();

					if (_CachedAbsoluteDest == null)
					{
						_CachedAbsoluteDest = new AbsolutePathReactiveDestination();
					}

					ReactionModel.Destination = _CachedAbsoluteDest;
				})
				.AddTo(_CompositeDisposable);


			// 3. ReactionModel.Destinationの更新に合わせてVMに変換
			DestinationVM = ReactionModel.ObserveProperty(x => x.Destination)
				.Select(ModelToVM)
				.ToReadOnlyReactiveProperty()
				.AddTo(_CompositeDisposable);

			
		}



		private void CacheDestinationModel()
		{
			if (ReactionModel.Destination is SameInputReactiveDestination)
			{
				_CachedSameInputDest = ReactionModel.Destination as SameInputReactiveDestination;
			}
			else if (ReactionModel.Destination is ChildReactiveDestination)
			{
				_CachedInputChildFolderDest = ReactionModel.Destination as ChildReactiveDestination;
			}
			else if (ReactionModel.Destination is AbsolutePathReactiveDestination)
			{
				_CachedAbsoluteDest = ReactionModel.Destination as AbsolutePathReactiveDestination;
			}
			else
			{
				throw new Exception();
			}
		}

		private DestinationViewModelBase ModelToVM(ReactiveDestinationBase destModel)
		{
			if (destModel is SameInputReactiveDestination)
			{
				return new SameInputDestinationViewModel(ReactionModel);
			} 
			else if (destModel is ChildReactiveDestination)
			{
				return new InputChildFolderDestinationViewModel(ReactionModel);
			}
			else if (destModel is AbsolutePathReactiveDestination)
			{
				return new AbsolutePathDestinationViewModel(ReactionModel);
			}
			else
			{
				throw new Exception();
			}
		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}
	}

	public class DestinationViewModelBase : BindableBase, IDisposable
	{
		public FolderReactionModel ReactionModel { get; private set; }

		protected CompositeDisposable _CompositeDisposable { get; private set; }




		public ReactiveProperty<string> OutputNamePattern { get; private set; }



		public DestinationViewModelBase(FolderReactionModel reactionModel)
		{
			ReactionModel = reactionModel;
			_CompositeDisposable = new CompositeDisposable();

			OutputNamePattern = reactionModel.Destination.ToReactivePropertyAsSynchronized(x => x.OutputNamePattern)
				.AddTo(_CompositeDisposable);

			
		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}
	}

	

	public class SameInputDestinationViewModel : DestinationViewModelBase
	{
		SameInputReactiveDestination Destination;

		public ReadOnlyReactiveProperty<string> OutputPathSample { get; private set; }


		public SameInputDestinationViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			Destination = reactionModel.Destination as SameInputReactiveDestination;


			OutputPathSample = Observable.CombineLatest(
					ReactionModel.ObserveProperty(x => x.WorkFolder).Select(x => x.FullName)
					, OutputNamePattern
				)
				.Select(x => {
					return Path.Combine(x[0], x[1]);
				})
				.ToReadOnlyReactiveProperty()
				.AddTo(_CompositeDisposable);
		}
	}
	public class InputChildFolderDestinationViewModel : DestinationViewModelBase
	{
		ChildReactiveDestination Destination;

		public ReactiveProperty<string> ChildFolderName { get; private set; }

		public ReadOnlyReactiveProperty<string> OutputPathSample { get; private set; }


		public InputChildFolderDestinationViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			Destination = reactionModel.Destination as ChildReactiveDestination;

			ChildFolderName = Destination.ToReactivePropertyAsSynchronized(x => x.ChildFolderName)
				.AddTo(_CompositeDisposable);

			OutputPathSample = Observable.CombineLatest(
					ReactionModel.ObserveProperty(x => x.WorkFolder).Select(x => x.FullName)
					, ChildFolderName
					, OutputNamePattern
				)
				.Select(x => {
					return Path.Combine(x[0], x[1], x[2]);
				})
				.ToReadOnlyReactiveProperty()
				.AddTo(_CompositeDisposable);
		}
	}

	public class AbsolutePathDestinationViewModel : DestinationViewModelBase
	{
		AbsolutePathReactiveDestination Destination;

		public string AbsoluteFolderPath { get; private set; }

		public ReadOnlyReactiveProperty<string> OutputPathSample { get; private set; }

		public AbsolutePathDestinationViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			Destination = reactionModel.Destination as AbsolutePathReactiveDestination;

			AbsoluteFolderPath = Destination.AbsoluteFolderPath;

			OutputPathSample = Observable.CombineLatest(
					Destination.ObserveProperty(x => x.AbsoluteFolderPath)
						.Where(x => false == String.IsNullOrWhiteSpace(x))
					, OutputNamePattern
				)
				.Select(x => {
					return Path.Combine(x[0], x[1]);
				})
				.ToReadOnlyReactiveProperty()
				.AddTo(_CompositeDisposable);
		}


		private DelegateCommand _SelectOutputFolderCommand;
		public DelegateCommand SelectOutputFolderCommand
		{
			get
			{
				return _SelectOutputFolderCommand
					?? (_SelectOutputFolderCommand = new DelegateCommand(() => 
					{
						// 出力先の絶対パスをFolder選択ダイアログを使って取得する
						OpenFileDialog ofp = new OpenFileDialog();
						ofp.FileName = "The file will be ignored";
						ofp.CheckFileExists = false;
						ofp.CheckPathExists = true;
						ofp.ValidateNames = false;

						try
						{
							var result = ofp.ShowDialog();
							if (result == DialogResult.OK &&
								false == String.IsNullOrWhiteSpace(ofp.FileName))
							{
								var folderPath = Path.GetDirectoryName(ofp.FileName);
								var folderInfo = new DirectoryInfo(folderPath);

								if (false == folderInfo.Exists)
								{
									return;
								}

								Destination.AbsoluteFolderPath = folderInfo.FullName;
								AbsoluteFolderPath = folderInfo.FullName;
								OnPropertyChanged(nameof(AbsoluteFolderPath));
							}
						}
						finally
						{
							ofp.Dispose();
						}


						
					}));
			}
		}
	}
}
