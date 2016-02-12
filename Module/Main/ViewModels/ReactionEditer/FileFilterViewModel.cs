using System;
using System.Linq;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Filters;
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





		public FileReactiveFilter FileFilterModel { get; private set; }
			

		public FileFilterViewModel()
			: base()
		{
			
		}


		public FileFilterViewModel(FolderReactionModel reactionModel, FileReactiveFilter filter)
			: base(reactionModel, filter)
		{
			FileFilterModel = filter;

			
		}



		

	}


}
