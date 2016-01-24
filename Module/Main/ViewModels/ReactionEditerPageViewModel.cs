using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using Modules.Main.Views;

namespace Modules.Main.ViewModels
{
	public class ReactionEditerPageViewModel : BindableBase, INavigationAware
	{
		private IRegionManager _RegionManager;
		public FolderReactionMonitorModel MonitorModel { get; private set; }

		private FolderReactionModel Reaction;



		public ReactiveProperty<string> Text { get; private set; }

		public ReactionEditerPageViewModel(IRegionManager regionManager, FolderReactionMonitorModel monitor)
		{
			_RegionManager = regionManager;
			MonitorModel = monitor;

			Text = new ReactiveProperty<string>("いやっっふうううううううぅ");
		}


		private void Initialize()
		{
			// TODO: initialize with this.Reaction
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
			// 対象となるFolderReactionGroupModelのGuidをパラメータから求める
			var reactionGuid = (Guid)navigationContext.Parameters["guid"];

			var reaction = MonitorModel.FindReaction(reactionGuid);

			if (reaction == null)
			{
				System.Diagnostics.Debug.WriteLine("error in ReactionEditerPageViewModel.OnNavigatedTo");
				System.Diagnostics.Debug.WriteLine("Reaction guid:" + reactionGuid.ToString());
				throw new Exception("invalid navigation parameter. not exist FolderReactionModel.");
			}

			// done
			Reaction = reaction;

			Initialize();
		}



		private DelegateCommand _BackCommand;
		public DelegateCommand BackCommand
		{
			get
			{
				return _BackCommand
					?? (_BackCommand = new DelegateCommand(() =>
					{
						_RegionManager.RequestNavigate("MainRegion", nameof(Views.FolderListPage));
					}));
			}
		}
	}
}
