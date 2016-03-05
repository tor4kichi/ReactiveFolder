using Prism.Commands;
using Prism.Mvvm;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.AppPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolderStyles.ViewModels
{
	public class AppOptionInstanceViewModel : BindableBase
	{
		public AppLaunchReactiveAction Action { get; private set; }
		public AppOptionInstance OptionInstance { get; private set; }

		public List<AppOptionValueViewModel> OptionValues { get; private set; }

		public string OptionName { get; private set; }

		public AppOptionInstanceViewModel(AppLaunchReactiveAction action, AppOptionInstance instance)
		{
			Action = action;
			OptionInstance = instance;


			OptionName = OptionInstance.OptionDeclaration.Name;

			OptionValues = OptionInstance.FromAppOptionInstance()
				.ToList();
		}

		private DelegateCommand _RemoveOptionCommand;
		public DelegateCommand RemoveOptionCommand
		{
			get
			{
				return _RemoveOptionCommand
					?? (_RemoveOptionCommand = new DelegateCommand(() =>
					{
						Action.RemoveAppOptionInstance(OptionInstance);
					}));
			}
		}
	}
}
