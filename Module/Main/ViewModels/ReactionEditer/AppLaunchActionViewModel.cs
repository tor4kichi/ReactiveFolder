using MaterialDesignThemes.Wpf;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.AppPolicy;
using ReactiveFolderStyles.DialogContent;
using ReactiveFolderStyles.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class AppLaunchActionViewModel : ActionViewModelBase
	{
		public ActionsEditViewModel EditVM { get; private set; }

		public AppLaunchReactiveAction Action { get; private set; }
		public ApplicationPolicy AppPolicy { get; private set; }

		public Guid AppGuid { get; private set; }

		public string AppName { get; private set; }

		public ReadOnlyReactiveCollection<AppOptionInstanceViewModel> UsingOptions { get; private set; }


		public AppLaunchActionViewModel(ActionsEditViewModel editVM, FolderReactionModel reactionModel, AppLaunchReactiveAction appAction)
			 : base(reactionModel)
		{
			EditVM = editVM;
			Action = appAction;

			AppPolicy = appAction.AppPolicy;
			if (AppPolicy != null)
			{
				AppName = AppPolicy.AppName;
				AppGuid = AppPolicy.Guid;

				UsingOptions = Action.AdditionalOptions.ToReadOnlyReactiveCollection(x =>
					new AppOptionInstanceViewModel(Action, x)
					);
			}
			else
			{
				AppName = "<App not found>";
			}
		}




		private DelegateCommand _RemoveActionCommand;
		public DelegateCommand RemoveActionCommand
		{
			get
			{
				return _RemoveActionCommand
					?? (_RemoveActionCommand = new DelegateCommand(() =>
					{
						// TODO: アクションの削除 確認ダイアログ
						EditVM.RemoveAction(this);
					}));
			}
		}


		public async Task<object> ShowSelectAppOptionDialog(AppPolicyOptionSelectDialogContentViewModel vm)
		{
			var view = new AppPolicyOptionSelectDialogContent()
			{
				DataContext = vm
			};

			return await DialogHost.Show(view, "ReactionEditCommonDialogHost");
		}


		private DelegateCommand _SelectAppOptionCommand;
		public DelegateCommand SelectAppOptionCommand
		{
			get
			{
				return _SelectAppOptionCommand
					?? (_SelectAppOptionCommand = new DelegateCommand(async () =>
					{
						var appPolicy = Action.AppPolicy;
						var optionDecls = appPolicy.OptionDeclarations
							.Where(x => Action.AdditionalOptions.All(alreadyAddedOption => x.Id != alreadyAddedOption.OptionId));
						var outputOptionDecls = appPolicy.OutputOptionDeclarations
							.Where(x => Action.AdditionalOptions.All(alreadyAddedOption => x.Id != alreadyAddedOption.OptionId));



						var optionItems = optionDecls.Select(x =>
							new ReactiveFolderStyles.DialogContent.AppPolicyOptionSelectItem()
							{
								OptionName = x.Name,
								OptionId = x.Id
							});

						var outputOptionItems = outputOptionDecls.Select(x =>
							new ReactiveFolderStyles.DialogContent.AppPolicyOptionSelectItem()
							{
								OptionName = x.Name,
								OptionId = x.Id
							});

						var dialogVM = new AppPolicyOptionSelectDialogContentViewModel(optionItems, outputOptionItems);

						var result = await ShowSelectAppOptionDialog(dialogVM);
						if (result != null && ((bool)result) == true)
						{
							foreach (var item in dialogVM.GetSelectedItems())
							{
								var decl = appPolicy.FindOptionDeclaration(item.OptionId);
								var instance = decl.CreateInstance();

								Action.AddAppOptionInstance(instance);
							}

						}

					}));
			}
		}
	}



	














}
