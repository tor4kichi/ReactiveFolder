using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.About.ViewModels
{
	public class AboutPageViewModel : BindableBase, INavigationAware
	{
		public IRegionManager _RegionManager;
		public IRegionNavigationService NavigationService;

		public List<TabViewModelBase> Tabs { get; private set; }

		public ReactiveProperty<TabViewModelBase> SelectedTab { get; private set; }



		public AboutPageViewModel(IRegionManager regionManagar)
		{
			_RegionManager = regionManagar;

			Tabs = new List<TabViewModelBase>();

			var aboutTabVM = new AboutTabViewModel();
			var lisenceTabVM = new LisenceTabViewModel();
			var howToTabVM = new HowToTabViewModel();
			Tabs.Add(aboutTabVM);
			Tabs.Add(howToTabVM);
			Tabs.Add(lisenceTabVM);



			SelectedTab = new ReactiveProperty<TabViewModelBase>(aboutTabVM);

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
							_RegionManager.RequestNavigate("MainRegion", "FolderListPage");
						}
					}));
			}
		}

		public void OnNavigatedTo(NavigationContext navigationContext)
		{
			NavigationService = navigationContext.NavigationService;
		}

		public bool IsNavigationTarget(NavigationContext navigationContext)
		{
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext)
		{
			// nothing do.
		}
	}

	abstract public class TabViewModelBase : BindableBase
	{
		abstract public string Title { get; }
	}


	public class AboutTabViewModel : TabViewModelBase
	{

		public override string Title { get; } = "What's this?";
	}

	public class HowToTabViewModel : TabViewModelBase
	{

		public override string Title { get; } = "How To";
	}


	public class LisenceTabViewModel : TabViewModelBase
	{
		public override string Title { get; } = "Using Library's";


		public List<LibraryItem> Libraries { get; private set; }


		public LisenceTabViewModel()
		{
			Libraries = new List<LibraryItem>()
			{
				new LibraryItem()
				{
					LibraryName = "Prism",
					AuthorName = ".NET Foundation",
					LisenceTypeName = LisenceType.Apache20.LisenceTypeToName(),
					SiteUri = "https://github.com/PrismLibrary/Prism"
				},
				new LibraryItem()
				{
					LibraryName = "ReactiveExtentions",
					AuthorName = "Microsoft",
					LisenceTypeName = LisenceType.Apache20.LisenceTypeToName(),
					SiteUri = "https://rx.codeplex.com/"
				},
				new LibraryItem()
				{
					LibraryName = "ReactiveProperty",
					AuthorName = "neuecc, xin9le, okazuki",
					LisenceTypeName = LisenceType.MIT.LisenceTypeToName(),
					SiteUri = "https://github.com/runceel/ReactiveProperty"
				},
				new LibraryItem()
				{
					LibraryName = "MaterialDesignInXamlToolkit",
					AuthorName = "James Willock",
					LisenceTypeName = LisenceType.MSPL.LisenceTypeToName(),
					SiteUri = "http://materialdesigninxaml.net/"
				},
				new LibraryItem()
				{
					LibraryName = "WPF Native Folder Browser",
					AuthorName = "hbarck",
					LisenceTypeName = LisenceType.MSPL.LisenceTypeToName(),
					SiteUri = "http://wpffolderbrowser.codeplex.com/"
				},
				new LibraryItem()
				{
					LibraryName = "Json.NET",
					AuthorName = "Newtonsoft",
					LisenceTypeName = LisenceType.MIT.LisenceTypeToName(),
					SiteUri = "http://www.newtonsoft.com/json"
				}
			};

			// ライブラリ名でソート
			Libraries.Sort(new Comparison<LibraryItem>(LibraryNameComperar));
		}


		int LibraryNameComperar(LibraryItem x, LibraryItem y)
		{
			return String.CompareOrdinal(x.LibraryName, y.LibraryName);
		}
	}



	public class LibraryItem
	{
		public string LibraryName { get; set; }
		public string AuthorName { get; set; }
		public string LisenceTypeName { get; set; }
		public string SiteUri { get; set; }
	}


	public enum LisenceType
	{
		Apache20,
		MIT,
		MSPL,
		GPLv3
	}	
	
	
	public static class LisenceTypeHelper
	{
		public static string LisenceTypeToName(this LisenceType lisence)
		{
			switch (lisence)
			{
				case LisenceType.Apache20:
					return "Apache License 2.0 (Apache)";
				case LisenceType.MIT:
					return "The MIT License (MIT)";
				case LisenceType.MSPL:
					return "Microsoft Public License (Ms-PL)";
				case LisenceType.GPLv3:
					return "GNU General Public License version 3(GPL v3)";
				default:
					break;
			}

			return lisence.ToString();
		}
	}	
}
