using Prism.Modularity;
using Prism.Regions;
using Modules.History.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Models;
using Prism.Events;
using ReactiveFolderStyles;
using ReactiveFolderStyles.Events;

namespace Modules.History
{
	public class HistoryModule : IModule
	{
		IEventAggregator _EveentAggregator;
		IRegionManager _regionManager;

		public HistoryModule(IRegionManager regionManager, IEventAggregator ea)
		{
			_regionManager = regionManager;
			_EveentAggregator = ea;
		}

		public void Initialize()
		{
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(HistoryPage));


			// open History
			var openReactionManageEvent = _EveentAggregator.GetEvent<PubSubEvent<OpenHisotryPageEventPayload>>();
			openReactionManageEvent.Subscribe(x =>
			{
				_regionManager.RequestNavigate("MainRegion", nameof(HistoryPage));
			}
			, keepSubscriberReferenceAlive: true);


			// open History with AppPolicy
			var openHisotryWithAppPolicyEvent = _EveentAggregator.GetEvent<PubSubEvent<OpenHisotryWithAppPolicyPageEventPayload>>();
			openHisotryWithAppPolicyEvent.Subscribe(x => 
			{
				var parameter = ViewModels.HisotryPageViewModel.CreateAppPolicyFilteringParameter(x.AppPolicyGuid);

				_regionManager.RequestNavigate("MainRegion", nameof(HistoryPage), parameter);
			}
			, keepSubscriberReferenceAlive:true);


			// open Hisotry with Reaction
			var openHistoryWithReactionEvent = _EveentAggregator.GetEvent<PubSubEvent<OpenHisotryWithReactionPageEventPayload>>();
			openHistoryWithReactionEvent.Subscribe(x =>
			{
				var parameter = ViewModels.HisotryPageViewModel.CreateReactionFilteringParameter(x.ReactionGuid);

				_regionManager.RequestNavigate("MainRegion", nameof(HistoryPage), parameter);
			}
			, keepSubscriberReferenceAlive: true);
		}
	}
}
