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
using ReactiveFolder.Models;
using Prism.Regions;
using ReactiveFolderStyles.Models;
using System.Windows;
using Prism.Interactivity.InteractionRequest;
using ReactiveFolderStyles.Events;

namespace ReactiveFolder.ViewModels
{
	public class MainWindowViewModel : BindableBase, IDisposable
	{
		public PageManager PageManager { get; private set; }
		public IRegionManager _RegionManager;

		public InteractionRequest<Notification> MessageRequest { get; private set; }


		private IFolderReactionMonitorModel _Monitor;
		private CompositeDisposable _CompositeDisposable;

		public ReactiveProperty<bool> IsOpenSideMenu { get; private set; }
		public ReactiveProperty<bool> IsOpenSubContent { get; private set; }


		public MainWindowViewModel(PageManager pageManager, IEventAggregator ea, IFolderReactionMonitorModel monitor, IRegionManager regionManagar)
		{
			PageManager = pageManager;
			_Monitor = monitor;
			_RegionManager = regionManagar;

			MessageRequest = new InteractionRequest<Notification>();

			_CompositeDisposable = new CompositeDisposable();


			var e = ea.GetEvent<PubSubEvent<TaskbarIconBalloonMessageEventPayload>>();
			e.Subscribe(x =>
			{
				MessageRequest.Raise(new Notification()
				{
					Title = x.Title,
					Content = x.Message
				});
			});

			IsOpenSubContent = PageManager
				.ObserveProperty(x => x.IsOpenSubContent)
				.ToReactiveProperty(false);

			IsOpenSideMenu = PageManager
				.ToReactivePropertyAsSynchronized(x => x.IsOpenSideMenu);
				
		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}

		private DelegateCommand _WindowActivatedCommand;
		public DelegateCommand WindowActivatedCommand
		{
			get
			{
				return _WindowActivatedCommand
					?? (_WindowActivatedCommand = new DelegateCommand(() =>
						{
//							_Monitor.RootFolder.UpdateReactionModels();
//							_Monitor.RootFolder.UpdateChildren();
	

							
						}
					));

			}
		}


		private DelegateCommand<object> _WindowSizeChangeCommand;
		public DelegateCommand<object> WindowSizeChangeCommand
		{
			get
			{
				return _WindowSizeChangeCommand
					?? (_WindowSizeChangeCommand = new DelegateCommand<object>((size) =>
					{
						if (size is Size)
						{
							var width = ((Size)size).Width;

							if (width >= 1280)
							{
								PageManager.SizeState = PageSizeState.XLarge;
							}
							else if (width >= 840)
							{
								PageManager.SizeState = PageSizeState.Large;
							}
							else if (width >= 600)
							{
								PageManager.SizeState = PageSizeState.Midium;
							}
							else if (width >= 480)
							{
								PageManager.SizeState = PageSizeState.Small;
							}
							else
							{
								PageManager.SizeState = PageSizeState.XSmall;
							}

						}
					}
					));

			}
		}

		private DelegateCommand _OpenWindowCommand;
		public DelegateCommand OpenWindowCommand
		{
			get
			{
				return _OpenWindowCommand
					?? (_OpenWindowCommand = new DelegateCommand(() =>
					{
						ShowWindow(App.Current.MainWindow);
					}
					));

			}
		}


		private void ShowWindow(Window win)
		{
			// ウィンドウ表示&最前面に持ってくる
			if (win.WindowState == System.Windows.WindowState.Minimized)
				win.WindowState = System.Windows.WindowState.Normal;

			win.Show();
			win.Activate();
			// タスクバーでの表示をする
			win.ShowInTaskbar = true;
		}

		private DelegateCommand _ExitApplicationCommand;
		public DelegateCommand ExitApplicationCommand
		{
			get
			{
				return _ExitApplicationCommand
					?? (_ExitApplicationCommand = new DelegateCommand(() =>
					{
						App.Current.Shutdown();

						
					}
					));

			}
		}
	}
}
