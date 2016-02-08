using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Model;

namespace Modules.Settings
{
	public class SettingsModule : IModule
	{
		IRegionManager _regionManager;

		public SettingsModule(IRegionManager regionManager)
		{
			_regionManager = regionManager;
		}

		public void Initialize()
		{
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(Settings.Views.SettingsPage));
		}
	}
}
