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


		private FolderReactionGroupModel ReactionGroup;
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
			var groupGuid = (Guid)navigationContext.Parameters["guid"];
			var group = MonitorModel.ReactionGroups.SingleOrDefault(x => x.Guid == groupGuid);
			if (group == null)
			{
				System.Diagnostics.Debug.WriteLine("error in ReactionEditerPageViewModel.OnNavigatedTo");
				System.Diagnostics.Debug.WriteLine("ReactionGroup guid:" + groupGuid.ToString());
				throw new Exception("invalid navigation parameter. not exist ReactionGroup");
			}

			// 対象となるFolderReactionModelをReactionIdを使って
			// 先に入手したGroupから求めるのReactionを
			var reactionId = (int)navigationContext.Parameters["reactionid"];
			var reaction = group.Reactions.SingleOrDefault(x => x.ReactionId == reactionId);
			if (reaction == null)
			{
				System.Diagnostics.Debug.WriteLine("error in ReactionEditerPageViewModel.OnNavigatedTo");
				System.Diagnostics.Debug.WriteLine("ReactionGroup guid:" +groupGuid.ToString());
				System.Diagnostics.Debug.WriteLine("Reaction Id:" + reactionId);
				throw new Exception("invalid navigation parameter. not exist Reaction in ReactionGroup");
			}


			// done
			ReactionGroup = group;
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
						var param = new NavigationParameters();
						param.Add("guid", this.ReactionGroup.Guid);
						_RegionManager.RequestNavigate("MainRegion", nameof(ReactionGroupEditerPage), param);
					}));
			}
		}
	}
}
