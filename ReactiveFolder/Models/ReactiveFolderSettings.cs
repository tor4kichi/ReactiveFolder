using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	public class ReactiveFolderSettings : IReactiveFolderSettings
	{
		public string ReactionSaveFolder { get; set; }
		public string AppPolicySaveFolder { get; set; }
		public string UpdateRecordSaveFolder { get; set; }

		public int DefaultMonitorIntervalSeconds { get; set; }


		public ReactiveFolderSettings()
		{
			
		}

		public void Load()
		{
			AppPolicySaveFolder = Properties.Settings.Default.AppPolicySaveFolder;
			UpdateRecordSaveFolder = Properties.Settings.Default.UpdateRecordSaveFolder;
			ReactionSaveFolder = Properties.Settings.Default.ReactionSaveFolder;
			DefaultMonitorIntervalSeconds = Properties.Settings.Default.DefaultMonitorIntervalSeconds;
		}

		public void Save()
		{
			Properties.Settings.Default.AppPolicySaveFolder = AppPolicySaveFolder;
			Properties.Settings.Default.UpdateRecordSaveFolder = UpdateRecordSaveFolder;
			Properties.Settings.Default.ReactionSaveFolder = ReactionSaveFolder;
			Properties.Settings.Default.DefaultMonitorIntervalSeconds = DefaultMonitorIntervalSeconds;

			Properties.Settings.Default.Save();
		}

	}
}
