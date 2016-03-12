using Prism.Events;
using Prism.Mvvm;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolderStyles.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToastNotifications;

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
				if (SetProperty(ref _PageType, value))
				{
					IsOpenSideMenu = false;
					IsOpenSubContent = false;
				}
			}
		}

		private bool _IsOpenSideMenu;
		public bool IsOpenSideMenu
		{
			get
			{
				return _IsOpenSideMenu;
			}
			set
			{
				SetProperty(ref _IsOpenSideMenu, value);
			}
		}

		private bool _IsOpenSubContent;
		public bool IsOpenSubContent
		{
			get
			{
				return _IsOpenSubContent;
			}
			set
			{
				SetProperty(ref _IsOpenSubContent, value);
			}
		}


		private PageSizeState _SizeState;
		public PageSizeState SizeState
		{
			get
			{
				return _SizeState;
			}
			set
			{
				SetProperty(ref _SizeState, value);
			}
		}






		public PageManager(IEventAggregator ea)
		{
			EventAggregator = ea;


			var subContentVisibleEvent = EventAggregator.GetEvent<PubSubEvent<SubContentVisibilityChangeEventPayload>>();
			subContentVisibleEvent.Subscribe(e => IsOpenSubContent = e.IsVisible);
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
				case AppPageType.History:
					OpenHistory();
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


		public void OpenReaction(string filePath)
		{
			if (String.IsNullOrEmpty(filePath) || false == File.Exists(filePath))
			{
				return;
			}

			if (false == filePath.EndsWith(FolderModel.REACTION_EXTENTION))
			{
				return;
			}

			var e = EventAggregator.GetEvent<PubSubEvent<OpenReactionWithFilePathEventPayload>>();
			e.Publish(new OpenReactionWithFilePathEventPayload()
			{
				FilePath = filePath
			});

			PageType = AppPageType.ReactionManage;
			IsOpenSubContent = true;
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

		public void OpenAppPolicy(Guid appPolicyGuid)
		{
			var e = EventAggregator.GetEvent<PubSubEvent<OpenAppPolicyWithAppGuidEventPayload>>();
			e.Publish(new OpenAppPolicyWithAppGuidEventPayload()
			{
				AppPolicyGuid = appPolicyGuid
			});



			PageType = AppPageType.AppPolicyManage;
		}





		public void OpenInstantAction()
		{
			var e = EventAggregator.GetEvent<PubSubEvent<OpenInstantActionEventPayload>>();
			e.Publish(new OpenInstantActionEventPayload());

			PageType = AppPageType.InstantAction;
		}

		public void OpenInstantActionWithInstantActionFile(string filePath)
		{
			if (String.IsNullOrEmpty(filePath) || false == File.Exists(filePath))
			{
				return;
			}

			var e = EventAggregator.GetEvent<PubSubEvent<OpenInstantActionWithFilePathEventPayload>>();
			e.Publish(new OpenInstantActionWithFilePathEventPayload()
			{
				FilePath = filePath
			});

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

		




		public void OpenHistory()
		{
			var e = EventAggregator.GetEvent<PubSubEvent<OpenHisotryPageEventPayload>>();
			e.Publish(new OpenHisotryPageEventPayload()
			{
				
			});

			PageType = AppPageType.History;
		}


		public void OpenHistoryWithAppPolicy(Guid appPolicyGuid)
		{
			var e = EventAggregator.GetEvent<PubSubEvent<OpenHisotryWithAppPolicyPageEventPayload>>();
			e.Publish(new OpenHisotryWithAppPolicyPageEventPayload()
			{
				AppPolicyGuid = appPolicyGuid
			});

			PageType = AppPageType.History;
		}

		public void OpenHistoryWithReaction(Guid reactionGuid)
		{
			var e = EventAggregator.GetEvent<PubSubEvent<OpenHisotryWithReactionPageEventPayload>>();
			e.Publish(new OpenHisotryWithReactionPageEventPayload()
			{
				ReactionGuid = reactionGuid
			});

			PageType = AppPageType.History;
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





		
		public void ShowInformation(string message)
		{
			var e = EventAggregator.GetEvent<PubSubEvent<ToastNotificationEventPayload>>();
			e.Publish(new ToastNotificationEventPayload()
			{
				Message = message,
				Type = NotificationType.Information
			});
		}

		public void ShowSuccess(string message)
		{
			var e = EventAggregator.GetEvent<PubSubEvent<ToastNotificationEventPayload>>();
			e.Publish(new ToastNotificationEventPayload()
			{
				Message = message,
				Type = NotificationType.Success
			});
		}

		public void ShowWarning(string message)
		{
			var e = EventAggregator.GetEvent<PubSubEvent<ToastNotificationEventPayload>>();
			e.Publish(new ToastNotificationEventPayload()
			{
				Message = message,
				Type = NotificationType.Warning
			});
		}

		public void ShowError(string message)
		{
			var e = EventAggregator.GetEvent<PubSubEvent<ToastNotificationEventPayload>>();
			e.Publish(new ToastNotificationEventPayload()
			{
				Message = message,
				Type = NotificationType.Error
			});
		}

	}

	public enum AppPageType
	{
		ReactionManage,
		AppPolicyManage,
		InstantAction,
		History,

		Settings,
		About,
	}


	public enum PageSizeState
	{
		XSmall,
		Small,
		Midium,
		Large,
		XLarge,
	}

	public static class PageManagerObservableExtention
	{
		public static IObservable<PageSizeState> ObservePageSizeSmallerThan(this PageManager pageManager, PageSizeState sizeState)
		{
			return pageManager.ObserveProperty(x => x.SizeState)
				.Where(x => x <= sizeState);
		}

		public static IObservable<PageSizeState> ObservePageSizeGreaterThan(this PageManager pageManager, PageSizeState sizeState)
		{
			return pageManager.ObserveProperty(x => x.SizeState)
				.Where(x => x >= sizeState);
		}

		public static IObservable<PageSizeState> ObservePageSizeEqual(this PageManager pageManager, PageSizeState sizeState)
		{
			return pageManager.ObserveProperty(x => x.SizeState)
				.Where(x => x == sizeState);
		}

		public static IObservable<PageSizeState> ObservePageSizeNotEqual(this PageManager pageManager, PageSizeState sizeState)
		{
			return pageManager.ObserveProperty(x => x.SizeState)
				.Where(x => x != sizeState);
		}
	}

}
