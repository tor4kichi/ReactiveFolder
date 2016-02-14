using Prism.Modularity;
using Prism.Regions;
using Modules.Monitor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Models;

namespace Modules.Monitor
{
	public class MonitorModule : IModule
	{
		IRegionManager _regionManager;

		IFolderReactionMonitorModel _MonitorModel;

		public MonitorModule(IRegionManager regionManager, IFolderReactionMonitorModel monitorModel)
		{
			_regionManager = regionManager;
			_MonitorModel = monitorModel;
		}

		public void Initialize()
		{
			_MonitorModel.DefaultInterval = TimeSpan.FromHours(1);

			_MonitorModel.Start();

		}
	}
}
