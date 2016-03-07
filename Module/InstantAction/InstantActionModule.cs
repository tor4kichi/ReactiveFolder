using Prism.Modularity;
using Prism.Regions;
using Modules.InstantAction.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Models;
using Prism.Events;
using ReactiveFolderStyles.Events;

namespace Modules.InstantAction
{
	public class InstantActionModule : IModule
	{
		IEventAggregator _EveentAggregator;
		IRegionManager _regionManager;

		public InstantActionModule(IRegionManager regionManager, IEventAggregator ea)
		{
			_regionManager = regionManager;
			_EveentAggregator = ea;
		}

		public void Initialize()
		{
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(Views.InstantActionPage));

			var openInstantActionEvent = _EveentAggregator.GetEvent<PubSubEvent<OpenInstantActionEventPayload>>();
			openInstantActionEvent.Subscribe(x =>
			{
				_regionManager.RequestNavigate("MainRegion", nameof(Views.InstantActionPage));
			}
			, keepSubscriberReferenceAlive: true);


			var openInstantActionWithFilePathEvent = _EveentAggregator.GetEvent<PubSubEvent<OpenInstantActionWithFilePathEventPayload>>();
			openInstantActionWithFilePathEvent.Subscribe(x =>
			{
				var param = ViewModels.InstantActionPageViewModel.MakeNavigationParamWithFilePath(x.FilePath);

				_regionManager.RequestNavigate("MainRegion", nameof(Views.InstantActionPage), param);
			}
			, keepSubscriberReferenceAlive: true);




			var openInstantActionWithTargetFilesEvent = _EveentAggregator.GetEvent<PubSubEvent<OpenInstantActionWithFilesEventPayload>>();
			openInstantActionWithTargetFilesEvent.Subscribe(x =>
			{
				var param = ViewModels.InstantActionPageViewModel.MakeNavigationParamWithTargetFiles(x.FilePaths);

				_regionManager.RequestNavigate("MainRegion", nameof(Views.InstantActionPage), param);
			}
			, keepSubscriberReferenceAlive: true);
		}
	}
}
