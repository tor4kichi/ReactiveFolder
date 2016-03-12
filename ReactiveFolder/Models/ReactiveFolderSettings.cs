using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	public class ReactiveFolderSettings : IReactiveFolderSettings
	{
		public string SaveFolder { get; set; }

		public int DefaultMonitorIntervalSeconds { get; set; }

		public int HistoryAvailableStorageSizeMB { get; set; }

		public ReactiveFolderSettings()
		{
			
		}

		public void Load()
		{
			SaveFolder = Properties.Settings.Default.SaveFolder;
			DefaultMonitorIntervalSeconds = Properties.Settings.Default.DefaultMonitorIntervalSeconds;
			HistoryAvailableStorageSizeMB = Properties.Settings.Default.HistoryAvailableStorageSizeMB;
		}

		public void Save()
		{
			Properties.Settings.Default.SaveFolder = SaveFolder;
			Properties.Settings.Default.DefaultMonitorIntervalSeconds = DefaultMonitorIntervalSeconds;
			Properties.Settings.Default.HistoryAvailableStorageSizeMB = HistoryAvailableStorageSizeMB;

			Properties.Settings.Default.Save();
		}

	}
}
