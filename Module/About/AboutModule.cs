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

namespace Modules.About
{
	public class AboutModule : IModule
	{
		IEventAggregator _EveentAggregator;
		IRegionManager _regionManager;

		public AboutModule(IRegionManager regionManager, IEventAggregator ea)
		{
			_regionManager = regionManager;
			_EveentAggregator = ea;
		}

		public void Initialize()
		{
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(About.Views.AboutPage));


			var settingsOpenEvent = _EveentAggregator.GetEvent<PubSubEvent<OpenAboutEventPayload>>();
			settingsOpenEvent.Subscribe(x =>
			{
				_regionManager.RequestNavigate("MainRegion", nameof(Views.AboutPage));
			}
			, keepSubscriberReferenceAlive: true);
		}
	}
}
