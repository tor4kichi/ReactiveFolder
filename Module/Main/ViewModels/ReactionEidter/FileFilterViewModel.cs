using System;
using System.Linq;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using ReactiveFolder.Model.Filters;
using System.Reactive.Linq;
using System.Collections.ObjectModel;


namespace Modules.Main.ViewModels.ReactionEditer
{
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


}
