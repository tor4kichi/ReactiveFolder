using Prism.Modularity;
using Prism.Regions;
using ReactiveFolder.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainModule
{
	public class ModuleAModule : IModule
	{
		IRegionManager _regionManager;

		public ModuleAModule(IRegionManager regionManager)
		{
			_regionManager = regionManager;
		}

		public void Initialize()
		{
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(FolderListPage));
		}
	}
}
