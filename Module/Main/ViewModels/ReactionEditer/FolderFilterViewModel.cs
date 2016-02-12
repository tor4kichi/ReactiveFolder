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

		public FolderReactiveFilter FolderFilter { get; private set; }


		public FolderFilterViewModel()
			: base()
		{
			
		}


		public FolderFilterViewModel(FolderReactionModel reactionModel, FolderReactiveFilter filter)
			: base(reactionModel, filter)
		{
			FolderFilter = filter;			
		}

	}

}
