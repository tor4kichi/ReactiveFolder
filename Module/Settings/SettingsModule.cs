using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Models;
using Prism.Events;
using ReactiveFolderStyles.Events;

namespace Modules.Settings
{
	public class SettingsModule : IModule
	{
		IEventAggregator _EveentAggregator;
		IRegionManager _regionManager;

		public SettingsModule(IRegionManager regionManager, IEventAggregator ea)
		{
			_regionManager = regionManager;
			_EveentAggregator = ea;
		}

		public void Initialize()
		{
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(Settings.Views.SettingsPage));

			var settingsOpenEvent = _EveentAggregator.GetEvent<PubSubEvent<OpenSettingsEventPayload>>();
			settingsOpenEvent.Subscribe(x =>
			{
				_regionManager.RequestNavigate("MainRegion", nameof(Views.SettingsPage));
			}
			, keepSubscriberReferenceAlive: true);
		}
	}
}
