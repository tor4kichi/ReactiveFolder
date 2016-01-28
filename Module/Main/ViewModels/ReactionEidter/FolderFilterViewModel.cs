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

}
