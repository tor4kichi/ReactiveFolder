﻿using Prism.Modularity;
using Prism.Regions;
using Modules.Main.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Models;

namespace Modules.Main
{
	[ModuleDependency("MonitorModule")]
	public class MainModule : IModule
	{
		IRegionManager _regionManager;

		public MainModule(IRegionManager regionManager)
		{
			_regionManager = regionManager;
		}

		public void Initialize()
		{
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(FolderReactionManagePage));
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(ReactionEditControl));

			_regionManager.RequestNavigate("MainRegion", nameof(FolderReactionManagePage));
		}
	}
}
