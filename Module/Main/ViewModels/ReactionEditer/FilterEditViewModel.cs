using Modules.Main.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Filters;
using ReactiveFolder.Models.Util;
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

		public List<FilterViewModelBase> Filters { get; private set; }

		public ReactiveProperty<FilterViewModelBase> SelectedFilterVM { get; private set; }


		public FilterEditViewModel(FolderReactionModel reactionModel)
			: base(@"Filter", reactionModel)
		{
			Reaction.ObserveProperty(x => x.IsFilterValid)
				.Subscribe(x => IsValid.Value = x)
				.AddTo(_CompositeDisposable);


			// 
			// Reactionが持っていないフィルターモデルは個々で作成する


			var currentFilterType = Reaction.Filter.OutputItemType;

			FileFilterViewModel fileFilterVM;
			FolderFilterViewModel folderFilterVM;

			if (currentFilterType == FolderItemType.File)
			{
				fileFilterVM = new FileFilterViewModel(Reaction, Reaction.Filter as FileReactiveFilter);
				folderFilterVM = new FolderFilterViewModel(Reaction, new FolderReactiveFilter());
			}
			else if (currentFilterType == FolderItemType.Folder)
			{
				fileFilterVM = new FileFilterViewModel(Reaction, new FileReactiveFilter());
				folderFilterVM = new FolderFilterViewModel(Reaction, Reaction.Filter as FolderReactiveFilter);
			}
			else
			{
				throw new Exception();
			}

			Filters = new List<FilterViewModelBase>();
			Filters.Add(fileFilterVM);
			Filters.Add(folderFilterVM);


			SelectedFilterVM = Reaction.ToReactivePropertyAsSynchronized(x => x.Filter,
				convert: (model) => Filters.Single(y => y.Filter == model),
				convertBack: (vm) => vm.Filter
				);
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
