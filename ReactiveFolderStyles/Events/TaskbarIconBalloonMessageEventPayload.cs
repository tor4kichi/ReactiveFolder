using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolderStyles.Events
{
	public class TaskbarIconBalloonMessageEventPayload
	{
		public string Title { get; set; } = "";
		public string Message { get; set; } = "";
		public System.Windows.Forms.ToolTipIcon Icon { get; set; } = System.Windows.Forms.ToolTipIcon.None;
	}
}
