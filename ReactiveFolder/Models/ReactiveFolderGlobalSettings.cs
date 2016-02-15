using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	public class ReactiveFolderGlobalSettings
	{
		public string ReactionSaveFolder { get; set; }
		public string AppPolicySaveFolder { get; set; }
		public string UpdateRecordSaveFolder { get; set; }

		public int DefaultMonitorIntervalSeconds { get; set; }
	}
}
