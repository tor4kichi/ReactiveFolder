using Modules.Main.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using ReactiveFolder.Model.Filters;
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
	public class FilterEditViewModel : ReactionEditViewModelBase
	{


		/// <summary>
		/// FileまたはFolderのフィルターモデルタイプ
		/// </summary>
		public ReactiveProperty<ReactionFilterType> FilterType { get; private set; }



		public FileFilterViewModel FileFilterVM { get; private set; }

		public FolderFilterViewModel FolderFilterVM { get; private set; }



		private ReactiveProperty<bool> _IsValid;
		public override ReactiveProperty<bool> IsValid
		{
			get
			{
				return _IsValid;
			}
		}



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




		public FilterEditViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			_IsValid = Reaction.ObserveProperty(x => x.IsFilterValid)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);



			var currentFilterType = FilterModelToVMType(Reaction.Filter);

			FilterType = new ReactiveProperty<ReactionFilterType>(currentFilterType)
				.AddTo(_CompositeDisposable);

		


			if (currentFilterType == ReactionFilterType.Files)
			{
				FileFilterVM = new FileFilterViewModel(Reaction, Reaction.Filter as FileReactiveFilter);
				FolderFilterVM = new FolderFilterViewModel(Reaction, new FolderReactiveFilter());
			}
			else if (currentFilterType == ReactionFilterType.Folder)
			{
				FileFilterVM = new FileFilterViewModel(Reaction, new FileReactiveFilter());
				FolderFilterVM = new FolderFilterViewModel(Reaction, Reaction.Filter as FolderReactiveFilter);
			}
			else
			{
				throw new Exception();
			}


			

			IsFileFilterSelected = new ReactiveProperty<bool>(currentFilterType == ReactionFilterType.Files)
				.AddTo(_CompositeDisposable);
			IsFileFilterSelected
				.Where(x => x)
				.Subscribe(_ =>
				{
					FilterType.Value = ReactionFilterType.Files;

					Reaction.Filter = FileFilterVM.FileFilterModel;
				})
				.AddTo(_CompositeDisposable);

			IsFolderFilterSelected = new ReactiveProperty<bool>(currentFilterType == ReactionFilterType.Folder)
				.AddTo(_CompositeDisposable);
			IsFolderFilterSelected
				.Where(x => x)
				.Subscribe(_ =>
				{
					FilterType.Value = ReactionFilterType.Folder;

					Reaction.Filter = FolderFilterVM.FolderFilter;
				})
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
		
		public override void Dispose()
		{
			base.Dispose();
		}

		
	}



	
}
