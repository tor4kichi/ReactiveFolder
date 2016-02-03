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






		// **************************

		// TODO: FileFilterを手動追加するためのテキストボックスを追加

		// **************************



		
		public ReactiveProperty<string> IncludeFilterText { get; private set; }

		public ReactiveProperty<string> ExcludeFilterText { get; private set; }



		/// <summary>
		/// 現在追加されているFileのフィルターパターン
		/// Model -> ViewModelへの一方通行
		/// FilterParttersを変更する場合はFileReactiveFilterに追加削除を行う。
		/// </summary>
		public ReadOnlyReactiveCollection<string> IncludeFilterPatterns { get; private set; }


		/// <summary>
		/// 現在追加されているFileのフィルターパターン
		/// Model -> ViewModelへの一方通行
		/// FilterParttersを変更する場合はFileReactiveFilterに追加削除を行う。
		/// </summary>
		public ReadOnlyReactiveCollection<string> ExcludeFilterPatterns { get; private set; }

	
		/// <summary>
		/// FilterPartternsをもとにFolderReactionModel.WorkFolder内の
		/// ファイルをフィルタリングした結果のファイル名（拡張子含む）
		/// </summary>
		public ObservableCollection<string> SampleItems { get; private set; }



		public FileReactiveFilter FileFilterModel { get; private set; }
			

		public FileFilterViewModel()
			: base(null)
		{
			var temp = new ObservableCollection<string>();
			IncludeFilterPatterns = temp.ToReadOnlyReactiveCollection();
			SampleItems = new ObservableCollection<string>();
		}


		public FileFilterViewModel(FolderReactionModel reactionModel, FileReactiveFilter filter)
			: base(reactionModel)
		{
			FileFilterModel = filter;

			IncludeFilterText = new ReactiveProperty<string>("");

			IncludeFilterPatterns = FileFilterModel.IncludeFilter
				.ToReadOnlyReactiveCollection()
				.AddTo(_CompositeDisposable);



			ExcludeFilterText = new ReactiveProperty<string>("");

			ExcludeFilterPatterns = FileFilterModel.ExcludeFilter
				.ToReadOnlyReactiveCollection()
				.AddTo(_CompositeDisposable);



			SampleItems = new ObservableCollection<string>(FileFilterModel.FileFilter(ReactionModel.WorkFolder).Select(x => x.Name));

			FileFilterModel.IncludeFilter
				.CollectionChangedAsObservable()
				.Subscribe(_ =>
				{
					SampleItems.Clear();
					SampleItems.AddRange(
						FileFilterModel.FileFilter(ReactionModel.WorkFolder).Select(x => x.Name)
						);
				})
				.AddTo(_CompositeDisposable);
		}



		// FiterText AddCommand

		private DelegateCommand<string> _AddIncludeFilterTextCommand;
		public DelegateCommand<string> AddIncludeFilterTextCommand
		{
			get
			{
				return _AddIncludeFilterTextCommand
					?? (_AddIncludeFilterTextCommand = new DelegateCommand<string>(word =>
					{
						FileFilterModel.AddIncludeFilter(word);

						FileFilterModel.Validate();

						IncludeFilterText.Value = "";
					}));
			}
		}



		private DelegateCommand<string> _RemoveIncludeFilterTextCommand;
		public DelegateCommand<string> RemoveIncludeFilterTextCommand
		{
			get
			{
				return _RemoveIncludeFilterTextCommand
					?? (_RemoveIncludeFilterTextCommand = new DelegateCommand<string>(word =>
					{
						FileFilterModel.RemoveInlcudeFilter(word);

						FileFilterModel.Validate();

						
					}));
			}
		}







		private DelegateCommand<string> _AddExcludeFilterTextCommand;
		public DelegateCommand<string> AddExcludeFilterTextCommand
		{
			get
			{
				return _AddExcludeFilterTextCommand
					?? (_AddExcludeFilterTextCommand = new DelegateCommand<string>(word =>
					{
						FileFilterModel.AddExcludeFilter(word);

						FileFilterModel.Validate();

						ExcludeFilterText.Value = "";
					}));
			}
		}



		private DelegateCommand<string> _RemoveExcludeFilterTextCommand;
		public DelegateCommand<string> RemoveExcludeFilterTextCommand
		{
			get
			{
				return _RemoveExcludeFilterTextCommand
					?? (_RemoveExcludeFilterTextCommand = new DelegateCommand<string>(word =>
					{
						FileFilterModel.RemoveExcludeFilter(word);

						FileFilterModel.Validate();
					}));
			}
		}

	}


}
