using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	// FilterEditViewModelが管理する対象となるFileVMとFolderVMのベース


	public class FilterViewModelBase : BindableBase, IDisposable
	{
		public FolderReactionModel ReactionModel { get; private set; }

		protected CompositeDisposable _CompositeDisposable { get; private set; }


		public FilterViewModelBase(FolderReactionModel reactionModel)
		{
			ReactionModel = reactionModel;
			_CompositeDisposable = new CompositeDisposable();
		}



		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}

	}
}
