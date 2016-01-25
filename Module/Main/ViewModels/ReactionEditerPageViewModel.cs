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
	public class ReactionEditerPageViewModel : PageViewModelBase, INavigationAware
	{
		private FolderReactionModel Reaction;

		public IRegionNavigationService NavigationService;

		public ReactiveProperty<string> Text { get; private set; }

		public ReactionEditerPageViewModel(IRegionManager regionManager, IRegionNavigationService navService, FolderReactionMonitorModel monitor)
			: base(regionManager, monitor)
		{
			Text = new ReactiveProperty<string>("いやっっふうううううううぅ");
		}


		private void Initialize()
		{
			// TODO: initialize with this.Reaction
			Text.Value = Reaction.Name;
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
			Reaction = base.ReactionModelFromNavigationParameters(navigationContext.Parameters);

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
						if (NavigationService.Journal.CanGoBack)
						{
							NavigationService.Journal.GoBack();
						}
						else
						{
							this.NavigationToFolderListPage();
						}
					}));
			}
		}
	}
}
