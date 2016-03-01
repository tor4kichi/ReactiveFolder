using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.InstantAction.ViewModels
{
	class InstantActionPageViewModel : BindableBase, INavigationAware
	{
		public IRegionNavigationService NavigationService;


		public InstantActionPageViewModel()
		{

		}

		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
			
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			NavigationService = navigationContext.NavigationService;
			
			try
			{
				var path = (string)navigationContext.Parameters["path"];

				if (path != null)
				{
					var fileInfo = new FileInfo(path);

				}
			}
			catch
			{

			}
		}

		public static NavigationParameters MakeNavigationParamWithTargetFile(string path)
		{
			var param = new NavigationParameters();
			param.Add("path", path);

			return param;
		}
	}
}
