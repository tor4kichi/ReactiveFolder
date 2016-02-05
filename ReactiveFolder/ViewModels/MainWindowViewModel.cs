using Prism.Mvvm;
using Prism.Events;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using Reactive.Bindings.Extensions;
using ReactiveFolderStyles;
using Prism.Commands;

namespace ReactiveFolder.ViewModels
{
	public class MainWindowViewModel : BindableBase, IDisposable
	{
		public ReactiveProperty<bool> IsOpenSideMenu { get; private set; }

		private CompositeDisposable _CompositeDisposable;

		public MainWindowViewModel(IEventAggregator ea)
		{
			_CompositeDisposable = new CompositeDisposable();

			IsOpenSideMenu = new ReactiveProperty<bool>(false)
				 .AddTo(_CompositeDisposable);

			ea.GetEvent<PubSubEvent<ReactiveFolderPageType>>()
				.Subscribe(x =>
				{
					IsOpenSideMenu.Value = true;
				}
				)
			.AddTo(_CompositeDisposable);
		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}

		private DelegateCommand _CloseSideMenuCommand;
		public DelegateCommand CloseSideMenuCommand
		{
			get
			{
				return _CloseSideMenuCommand
					?? (_CloseSideMenuCommand = new DelegateCommand(() =>
						CloseSideMenu()
					));
			}
		}

		public void CloseSideMenu()
		{
			IsOpenSideMenu.Value = false;
		}
	}
}
