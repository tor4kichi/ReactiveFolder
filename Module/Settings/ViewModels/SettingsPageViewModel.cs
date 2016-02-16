using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using ReactiveFolder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Settings.ViewModels
{
	public class SettingsPageViewModel : BindableBase, INavigationAware
	{
		public IRegionManager _RegionManager;
		public IRegionNavigationService NavigationService;

		public IReactiveFolderSettings Settings { get; private set; }


		public ReactiveProperty<string> ReactionCheckInterval { get; private set; }




		public SettingsPageViewModel(IRegionManager regionManagar, IReactiveFolderSettings settings)
		{
			_RegionManager = regionManagar;
			Settings = settings;

			ReactionCheckInterval = new ReactiveProperty<string>(settings.DefaultMonitorIntervalSeconds.ToString());

			ReactionCheckInterval.Subscribe(x =>
			{
				settings.DefaultMonitorIntervalSeconds = int.Parse(x);
				settings.Save();
			});

			ReactionCheckInterval.SetValidateNotifyError(x => 
			{
				int temp;
				if (false == int.TryParse(x, out temp))
				{
					return "Number Only";
				}

				return null;
			});

		}

		private DelegateCommand _BackCommand;
		public DelegateCommand BackCommand
		{
			get
			{
				return _BackCommand
					?? (_BackCommand = new DelegateCommand(() =>
					{
						if (NavigationService.Journal.CanGoBack)
						{
							NavigationService.Journal.GoBack();
						}
						else
						{
							_RegionManager.RequestNavigate("MainRegion", "FolderListPage");
						}
					}));
			}
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			NavigationService = navigationContext.NavigationService;
		}

		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
			// nothing do.
		}
	}
}
