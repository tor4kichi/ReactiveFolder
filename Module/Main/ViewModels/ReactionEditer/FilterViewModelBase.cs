using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	// FilterEditViewModelが管理する対象となるFileVMとFolderVMのベース


	abstract public class FilterViewModelBase : BindableBase, IDisposable
	{
		public FolderReactionModel ReactionModel { get; private set; }

		protected CompositeDisposable _CompositeDisposable { get; private set; }

		public ReactiveFilterBase Filter { get; private set; }

		public string FilterType { get; private set; }


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


		public FilterViewModelBase()
		{
			IncludeFilterText = new ReactiveProperty<string>("");
			ExcludeFilterText = new ReactiveProperty<string>("");

			var temp = new ObservableCollection<string>();
			IncludeFilterPatterns = temp.ToReadOnlyReactiveCollection();
			ExcludeFilterPatterns = temp.ToReadOnlyReactiveCollection();
			SampleItems = temp.ToReadOnlyReactiveCollection();
		}

		public FilterViewModelBase(FolderReactionModel reactionModel, ReactiveFilterBase filter)
		{
			ReactionModel = reactionModel;
			Filter = filter;
			_CompositeDisposable = new CompositeDisposable();

			FilterType = Filter.OutputItemType.ToString();


			IncludeFilterText = new ReactiveProperty<string>("");

			IncludeFilterPatterns = Filter.IncludeFilter
				.ToReadOnlyReactiveCollection()
				.AddTo(_CompositeDisposable);



			ExcludeFilterText = new ReactiveProperty<string>("");

			ExcludeFilterPatterns = Filter.ExcludeFilter
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



		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}



		private DelegateCommand<string> _AddIncludeFilterTextCommand;
		public DelegateCommand<string> AddIncludeFilterTextCommand
		{
			get
			{
				return _AddIncludeFilterTextCommand
					?? (_AddIncludeFilterTextCommand = new DelegateCommand<string>(word =>
					{
						Filter.AddIncludeFilter(word);

						Filter.Validate();

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
						Filter.RemoveInlcudeFilter(word);

						Filter.Validate();


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
						Filter.AddExcludeFilter(word);

						Filter.Validate();

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
						Filter.RemoveExcludeFilter(word);

						Filter.Validate();
					}));
			}
		}

	}
}
