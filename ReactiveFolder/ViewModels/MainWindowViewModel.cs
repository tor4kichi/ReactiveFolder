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

namespace ReactiveFolder.ViewModels
{
	public class MainWindowViewModel : BindableBase, IDisposable
	{
		public ReactiveFolderApp App { get; private set; }
		public IRegionManager _RegionManager;



		private IFolderReactionMonitorModel _Monitor;
		private CompositeDisposable _CompositeDisposable;

		public MainWindowViewModel(ReactiveFolderApp app, IEventAggregator ea, IFolderReactionMonitorModel monitor, IRegionManager regionManagar)
		{
			App = app;
			_Monitor = monitor;
			_RegionManager = regionManagar;

			_CompositeDisposable = new CompositeDisposable();

			App.ObserveProperty(x => x.PageType)
				.Subscribe(x =>
				{
					switch (x)
					{
						case AppPageType.AppPolicyManage:
							_RegionManager.RequestNavigate("MainRegion", nameof(Modules.AppPolicy.Views.AppPolicyManagePage));
							break;
						case AppPageType.ReactionManage:
							_RegionManager.RequestNavigate("MainRegion", nameof(Modules.Main.Views.FolderReactionManagePage));
							break;
						case AppPageType.Settings:
							_RegionManager.RequestNavigate("MainRegion", nameof(Modules.Settings.Views.SettingsPage));
							break;
						case AppPageType.About:
							_RegionManager.RequestNavigate("MainRegion", nameof(Modules.About.Views.AboutPage));
							break;
						case AppPageType.InstantAction:
							_RegionManager.RequestNavigate("MainRegion", nameof(Modules.InstantAction.Views.InstantActionPage));
							break;
						default:
							break;
					}
				})
				.AddTo(_CompositeDisposable);
				
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
							_Monitor.RootFolder.UpdateReactionModels();
							_Monitor.RootFolder.UpdateChildren();


							
						}
					));

			}
		}
	}
}
