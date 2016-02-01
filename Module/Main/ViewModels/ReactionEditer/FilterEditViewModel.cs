﻿using Modules.Main.Views;
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


		/// <summary>
		/// FileまたはFolderのフィルターViewModel
		/// FilterTypeに反応してVMを更新する
		/// </summary>
		public ReactiveProperty<FilterViewModelBase> FilterVM { get; private set; }


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



		// TODO: FilterVMをキャッシュして使いまわせるようにしたい

		/// <summary>
		/// FilterVMをちゃんと消すために使うDisposer
		/// FilterVMを使いまわさずに切り替えの都度生成しているため必要
		/// </summary>
		private IDisposable _FilterVMDisposer;


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
			: base(reactionModel)
		{
			_IsValid = Reaction.ObserveProperty(x => x.IsDestinationValid)
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);



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

		public override void Dispose()
		{
			_FilterVMDisposer?.Dispose();
			_FilterVMDisposer = null;

			base.Dispose();
		}

		
	}



	
}
