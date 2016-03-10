using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastNotifications;

namespace ReactiveFolderStyles.Events
{
	public class ToastNotificationEventPayload
	{
		public string Message { get; set; }
		public NotificationType Type { get; set; }
	}

	
}
