using Prism.Modularity;
using Prism.Regions;
using Modules.Main.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Models;
using Prism.Events;
using ReactiveFolderStyles;
using ReactiveFolderStyles.Events;

namespace Modules.Main
{
	[ModuleDependency("MonitorModule")]
	public class MainModule : IModule
	{
		IEventAggregator _EveentAggregator;
		IRegionManager _regionManager;

		public MainModule(IRegionManager regionManager, IEventAggregator ea)
		{
			_regionManager = regionManager;
			_EveentAggregator = ea;
		}

		public void Initialize()
		{
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(FolderReactionManagePage));



			var openReactionManageEvent = _EveentAggregator.GetEvent<PubSubEvent<OpenReactionManageEventPayload>>();
			openReactionManageEvent.Subscribe(x =>
			{
				_regionManager.RequestNavigate("MainRegion", nameof(FolderReactionManagePage));
			}
			, keepSubscriberReferenceAlive: true);



			var openReactionEvent = _EveentAggregator.GetEvent<PubSubEvent<OpenReactionEventPayload>>();
			openReactionEvent.Subscribe(x => 
			{
				var parameter = ViewModels.FolderReactionManagePageViewModel.CreateOpenReactionParameter(x.ReactionGuid);

				_regionManager.RequestNavigate("MainRegion", nameof(FolderReactionManagePage), parameter);
			}
			, keepSubscriberReferenceAlive:true);
		}
	}
}
