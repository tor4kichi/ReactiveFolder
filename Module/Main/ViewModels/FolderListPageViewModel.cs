﻿using Prism.Commands;
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
using ReactiveFolder.Model.Filters;
using ReactiveFolder.Model.Timings;
using ReactiveFolder.Model.Actions;
using ReactiveFolder.Model.Destinations;

namespace Modules.Main.ViewModels
{
	public class FolderListPageViewModel : BindableBase, INavigationAware
	{
		private IRegionManager _RegionManager;
		public FolderReactionMonitorModel MonitorModel { get; private set; }

		public ReadOnlyReactiveCollection<ReactiveFolderGroupViewModel> Groups { get; private set; }

		

		public FolderListPageViewModel(IRegionManager regionManager, FolderReactionMonitorModel monitor)
		{
			_RegionManager = regionManager;
			MonitorModel = monitor;

			Groups = MonitorModel.ReactionGroups
				.ToReadOnlyReactiveCollection(x => new ReactiveFolderGroupViewModel(this, x));
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
			// nothing to do.
		}



		private DelegateCommand _AddReactionFolderGroupCommand;
		public DelegateCommand AddReactionFolderGroupCommand
		{
			get
			{
				return _AddReactionFolderGroupCommand
					?? (_AddReactionFolderGroupCommand = new DelegateCommand(() => 
					{
						var desktop = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

						var targetDir = new System.IO.DirectoryInfo(desktop);

						var group = MonitorModel.CreateNewReactionGroup(targetDir);

						group.Name = "Test";

						var reaction = group.AddReaction();
						reaction.Name = "something reaction";

						reaction.Destination = new SameInputReactiveDestination();
						reaction.Timing = new FileUpdateReactiveTiming();
						reaction.Filter = new FileReactiveFilter();

						reaction.AddAction(new RenameReactiveAction("#{name}"));



						MonitorModel.SaveReactionGroup(group.Guid);

						NavigationToReactionGroupEditerPage(group);
					}));
			}
		}

		

		public void NavigationToReactionGroupEditerPage(FolderReactionGroupModel group)
		{
			var param = new NavigationParameters();
			param.Add("guid", group.Guid);
			this._RegionManager.RequestNavigate("MainRegion", nameof(Views.ReactionGroupEditerPage), param);
		}
	}


	public class ReactiveFolderGroupViewModel : BindableBase
	{
		public FolderListPageViewModel PageVM { get; private set; }
		public FolderReactionGroupModel GroupModel { get; private set; }
		public ReactiveProperty<string> Name { get; private set; }


		public ReactiveFolderGroupViewModel(FolderListPageViewModel parentVM, FolderReactionGroupModel model)
		{
			PageVM = parentVM;
			GroupModel = model;


			Name = GroupModel.ObserveProperty(x => x.Name)
				.ToReactiveProperty();
//			Name = model.ToReactiveProperty
		}


		private DelegateCommand _OpenReactionFolderGroupInEditer;
		public DelegateCommand OpenReactionFolderGroupInEditer
		{
			get
			{
				return _OpenReactionFolderGroupInEditer
					?? (_OpenReactionFolderGroupInEditer = new DelegateCommand(() =>
					{
						PageVM.NavigationToReactionGroupEditerPage(GroupModel);
					}));
			}
		}

	}
}
