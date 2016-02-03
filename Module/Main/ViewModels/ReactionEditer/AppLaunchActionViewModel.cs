using Microsoft.Practices.Prism.Commands;
using ReactiveFolder.Model;
using ReactiveFolder.Model.Actions;
using ReactiveFolder.Model.AppPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class AppLaunchActionViewModel : ActionViewModelBase
	{
		public static List<string> AppList;


		public string AppName { get; private set; }

		public string AppArgumentName { get; private set; }

		static AppLaunchActionViewModel()
		{
			AppList = AppLaunchReactiveAction.AppPolicyFactory.GetPolicies()
				.Select(x => x.AppName)
				.ToList();
		}

		// アプリの選択とアプリ内のAppOptionの選択

		public AppLaunchActionViewModel(FolderReactionModel reactionModel, AppLaunchReactiveAction appAction)
			 : base(reactionModel)
		{
			AppName = appAction.ApplicationName;

			AppArgumentName = appAction.ParamterSetName;
		}

		private DelegateCommand<string> _SelectAppCommand;
		public DelegateCommand<string> SelectAppCommand
		{
			get
			{
				return _SelectAppCommand
					?? (_SelectAppCommand = new DelegateCommand<string>((x) =>
					{
					}));
			}
		}

		private DelegateCommand<string> _SelectAppOptionCommand;
		public DelegateCommand<string> SelectAppOptionCommand
		{
			get
			{
				return _SelectAppOptionCommand
					?? (_SelectAppOptionCommand = new DelegateCommand<string>((x) =>
					{
					}));
			}
		}

	}
}
