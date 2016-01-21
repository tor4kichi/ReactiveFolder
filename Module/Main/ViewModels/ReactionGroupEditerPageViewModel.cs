using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;

namespace Modules.Main.ViewModels
{
	public class ReactionGroupEditerPageViewModel : BindableBase, INavigationAware
	{
		private IRegionManager _RegionManager;
		public FolderReactionMonitorModel MonitorModel { get; private set; }

		public FolderReactionGroupModel GroupModel { get; private set; }

		public ReactiveProperty<string> Name { get; private set; }

		public ReactionGroupEditerPageViewModel(IRegionManager regionManager, FolderReactionMonitorModel monitor)
		{
			_RegionManager = regionManager;
			MonitorModel = monitor;

			Name = new ReactiveProperty<string>("");

//			Name.Subscribe(x => GroupModel.Name = x);
		}

		public void Initialize()
		{
			Name.Value = GroupModel.Name;
		}


		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
			// nothing to do.
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			Guid requestGuid = (Guid)navigationContext.Parameters["guid"];

			var group = MonitorModel.ReactionGroups.SingleOrDefault(x => x.Guid == requestGuid);

			if (group == null)
			{
				throw new Exception();
			}

			this.GroupModel = group;

			Initialize();
		}
	}
}
