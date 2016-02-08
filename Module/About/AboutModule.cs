using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Model;

namespace Modules.About
{
	public class AboutModule : IModule
	{
		IRegionManager _regionManager;

		public AboutModule(IRegionManager regionManager)
		{
			_regionManager = regionManager;
		}

		public void Initialize()
		{
			_regionManager.RegisterViewWithRegion("MainRegion", typeof(About.Views.AboutPage));
		}
	}
}
