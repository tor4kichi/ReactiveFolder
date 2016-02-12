using Modules.Main.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace Modules.Main.ViewModels.ReactionEditer
{

	/// <summary>
	/// FilterEditViewModelに管理されるフォルダーフィルターモデル向けのVM
	/// フォルダは一つだけ選択できる。
	/// </summary>
	public class FolderFilterViewModel : FilterViewModelBase
	{
		public static readonly FolderFilterViewModel Empty = new FolderFilterViewModel();

		// Note: Folderは選択できるのは一つだけ


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
		/// FolderFilterPatternをFolderReactiveModel.WorkFolderのフォルダに
		/// 実際に適用した結果のフォルダ名
		/// </summary>
		public ReadOnlyReactiveCollection<string> SampleItems { get; private set; }




		public FolderReactiveFilter FolderFilter { get; private set; }


		public FolderFilterViewModel()
			: base(null)
		{
			IncludeFilterText = new ReactiveProperty<string>("");
			ExcludeFilterText = new ReactiveProperty<string>("");

			var temp = new ObservableCollection<string>();
			IncludeFilterPatterns = temp.ToReadOnlyReactiveCollection();
			ExcludeFilterPatterns = temp.ToReadOnlyReactiveCollection();
			SampleItems = temp.ToReadOnlyReactiveCollection();
		}


		public FolderFilterViewModel(FolderReactionModel reactionModel, FolderReactiveFilter filter)
			: base(reactionModel)
		{
			FolderFilter = filter;

			IncludeFilterText = new ReactiveProperty<string>("");

			IncludeFilterPatterns = FolderFilter.IncludeFilter
				.ToReadOnlyReactiveCollection()
				.AddTo(_CompositeDisposable);



			ExcludeFilterText = new ReactiveProperty<string>("");

			ExcludeFilterPatterns = FolderFilter.ExcludeFilter
				.ToReadOnlyReactiveCollection()
				.AddTo(_CompositeDisposable);



			/*
			SampleItems = FolderFilter.ObserveProperty(x => x.FolderFilterPattern)
				.Throttle(TimeSpan.FromSeconds(0.25))
				.SelectMany(x => FolderFilter.DirectoryFilter(ReactionModel.WorkFolder))
				.Select(x => $"/{x.Name}")
				.ToReadOnlyReactiveCollection()
				.AddTo(_CompositeDisposable);
				*/
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
						FolderFilter.AddIncludeFilter(word);

						FolderFilter.Validate();

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
						FolderFilter.RemoveInlcudeFilter(word);

						FolderFilter.Validate();


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
						FolderFilter.AddExcludeFilter(word);

						FolderFilter.Validate();

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
						FolderFilter.RemoveExcludeFilter(word);

						FolderFilter.Validate();
					}));
			}
		}


	}

}
