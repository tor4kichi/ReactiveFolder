using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using Microsoft.Practices.Prism.Commands;
using Modules.Main.Views;

namespace Modules.Main.ViewModels
{
	public class ReactionGroupEditerPageViewModel : BindableBase, INavigationAware
	{
		private IRegionManager _RegionManager;
		public FolderReactionMonitorModel MonitorModel { get; private set; }

		public FolderReactionGroupModel GroupModel { get; private set; }

		public ReactiveProperty<string> Name { get; private set; }

		public ReadOnlyReactiveCollection<FolderReactionListItemViewModel> ListItemReactions { get; private set; }

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

			ListItemReactions = GroupModel.Reactions
				.ToReadOnlyReactiveCollection(x =>
					new FolderReactionListItemViewModel(this, x)
				);
			OnPropertyChanged(nameof(ListItemReactions));
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



		private DelegateCommand _BackCommand;
		public DelegateCommand BackCommand
		{
			get
			{
				return _BackCommand
					?? (_BackCommand = new DelegateCommand(() =>
					{
						_RegionManager.RequestNavigate("MainRegion", nameof(FolderListPage));
					}));
			}
		}


		private DelegateCommand _AddReactionCommand;
		public DelegateCommand AddReactionCommand
		{
			get
			{
				return _AddReactionCommand
					?? (_AddReactionCommand = new DelegateCommand(() =>
					{
						var reaction = GroupModel.AddReaction();


						EditReaction(reaction);
					}));
			}
		}

		internal void EditReaction(FolderReactionModel reaction)
		{
			var param = new NavigationParameters();
			param.Add("guid", GroupModel.Guid);
			param.Add("reactionid", reaction.ReactionId);

			_RegionManager.RequestNavigate("MainRegion", nameof(ReactionEditerPage), param);

		}
	}


	public class FolderReactionListItemViewModel : BindableBase
	{
		ReactionGroupEditerPageViewModel PageVM;
		FolderReactionModel ReactionModel;


		public string Name { get; private set; }

		public FolderReactionListItemViewModel(ReactionGroupEditerPageViewModel pageVM, FolderReactionModel reaction)
		{
			PageVM = pageVM;
			ReactionModel = reaction;

			Name = ReactionModel.Name;
		}



		private DelegateCommand _EditCommand;
		public DelegateCommand EditCommand
		{
			get
			{
				return _EditCommand
					?? (_EditCommand = new DelegateCommand(() =>
					{
						PageVM.EditReaction(ReactionModel);
					}));
			}
		}
	}
}
