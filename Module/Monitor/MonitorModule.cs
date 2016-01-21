using Prism.Modularity;
using Prism.Regions;
using Modules.Monitor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Model;

namespace Modules.Monitor
{
	public class MonitorModule : IModule
	{
		IRegionManager _regionManager;

		FolderReactionMonitorModel _MonitorModel;

		public MonitorModule(IRegionManager regionManager, FolderReactionMonitorModel monitorModel)
		{
			_regionManager = regionManager;
			_MonitorModel = monitorModel;
		}

		public void Initialize()
		{
			_MonitorModel.Interval = TimeSpan.FromHours(1);
			_MonitorModel.Start();

		}
	}
}
