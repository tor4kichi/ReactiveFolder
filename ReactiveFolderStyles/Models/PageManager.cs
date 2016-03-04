using Prism.Events;
using Prism.Mvvm;
using ReactiveFolderStyles.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReactiveFolderStyles.Models
{

	// Note: ページ遷移を疎結合に実現するため、ページ遷移イベント送信をサポートする

	public class PageManager : BindableBase
	{
		public IEventAggregator EventAggregator { get; private set; }

		private AppPageType _PageType;
		public AppPageType PageType
		{
			get
			{
				return _PageType;
			}
			private set
			{
				SetProperty(ref _PageType, value);
			}
		}


		public PageManager(IEventAggregator ea)
		{
			EventAggregator = ea;
		}



		public void OpenPage(AppPageType pageType)
		{
			switch (pageType)
			{
				case AppPageType.ReactionManage:
					OpenReactionManage();
					break;
				case AppPageType.AppPolicyManage:
					OpenAppPolicyManage();
					break;
				case AppPageType.InstantAction:
					OpenInstantAction();
					break;
				case AppPageType.Settings:
					OpenSettings();
					break;
				case AppPageType.About:
					OpenAbout();
					break;
				default:
					throw new NotSupportedException("not support pagetype: " + pageType.ToString());
			}
		}

		public void OpenReactionManage()
		{
			var e = EventAggregator.GetEvent<PubSubEvent<OpenReactionManageEventPayload>>();
			e.Publish(new OpenReactionManageEventPayload());

			PageType = AppPageType.ReactionManage;
		}

		public void OpenReaction(Guid reactionGuid)
		{
			var e = EventAggregator.GetEvent<PubSubEvent<OpenReactionEventPayload>>();
			e.Publish(new OpenReactionEventPayload()
			{
				ReactionGuid = reactionGuid
			});

			PageType = AppPageType.ReactionManage;
		}




		public void OpenAppPolicyManage()
		{
			var e = EventAggregator.GetEvent<PubSubEvent<OpenAppPolicyManageEventPayload>>();
			e.Publish(new OpenAppPolicyManageEventPayload());

			PageType = AppPageType.AppPolicyManage;
		}




		public void OpenInstantAction()
		{
			var e = EventAggregator.GetEvent<PubSubEvent<OpenInstantActionEventPayload>>();
			e.Publish(new OpenInstantActionEventPayload());

			PageType = AppPageType.InstantAction;
		}


		public void OpenInstantActionWithDefaultFiles(string[] paths)
		{
			var e = EventAggregator.GetEvent<PubSubEvent<OpenInstantActionWithFilesEventPayload>>();
			e.Publish(new OpenInstantActionWithFilesEventPayload()
			{
				FilePaths = paths
			});

			PageType = AppPageType.InstantAction;
		}

		




		public void OpenSettings()
		{
			var e = EventAggregator.GetEvent<PubSubEvent<OpenSettingsEventPayload>>();
			e.Publish(new OpenSettingsEventPayload());

			PageType = AppPageType.Settings;
		}




		public void OpenAbout()
		{
			var e = EventAggregator.GetEvent<PubSubEvent<OpenAboutEventPayload>>();
			e.Publish(new OpenAboutEventPayload());

			PageType = AppPageType.About;
		}



		public void ShowTaskbarBalloonMessage(string title, string message, ToolTipIcon icon = ToolTipIcon.None)
		{
			var e = EventAggregator.GetEvent<PubSubEvent<TaskbarIconBalloonMessageEventPayload>>();
			e.Publish(new TaskbarIconBalloonMessageEventPayload()
			{
				Title = title,
				Message = message,
				Icon = icon
			});
		}

	}

	public enum AppPageType
	{
		ReactionManage,
		AppPolicyManage,
		InstantAction,

		Settings,
		About,
	}

}
