using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using ReactiveFolder.Models;
using ReactiveFolderStyles.Models;
using ReactiveFolderStyles.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Settings.ViewModels
{
	public class SettingsPageViewModel : PageViewModelBase
	{
		public IRegionNavigationService NavigationService;

		public IReactiveFolderSettings Settings { get; private set; }


		public ReactiveProperty<string> ReactionCheckInterval { get; private set; }




		public SettingsPageViewModel(PageManager pageManager, IReactiveFolderSettings settings)
			: base(pageManager)
		{
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


		
		public override void OnNavigatedTo(NavigationContext navigationContext)
		{
			
		}

		public override void OnNavigatedFrom(NavigationContext navigationContext)
		{
			
		}
	}
}
