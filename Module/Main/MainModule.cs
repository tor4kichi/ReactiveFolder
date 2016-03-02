using Prism.Modularity;
using Prism.Regions;
using Modules.Main.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Models;
using Prism.Events;
using ReactiveFolderStyles;

namespace Modules.Main
{
	[ModuleDependency("MonitorModule")]
	public class MainModule : IModule
	{
		IEventAggregator _EveentAggregator;
		IRegionManager _regionManager;

		public MainModule(IRegionManager regionManager, IEventAggregator ea)
		{
			_regionManager = regionManager;
			_EveentAggregator = ea;
		}

		public void Initialize()
		{
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(FolderReactionManagePage));
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(ReactionEditControl));
		}
	}
}
