using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels
{
	// ReactiveFolderModel.FolderModelに含まれるFolderReactionModelのVM
	public class ReactionListItemViewModel : BindableBase
	{
		public FolderReactionManagePageViewModel PageVM { get; private set; }

		public FolderReactionModel Reaction { get; private set; }

		public string Name { get; private set; }

		public string FilePath { get; private set; }


		public bool IsInactive { get; private set; }

		public bool IsInvalid { get; private set; }

		private bool _IsSelected;
		public bool IsSelected
		{
			get
			{
				return _IsSelected;
			}
			set
			{
				SetProperty(ref _IsSelected, value);
			}
		}


		public ReactionListItemViewModel(FolderReactionManagePageViewModel pageVM, FolderReactionModel reactionModel)
		{
			PageVM = pageVM;
			Reaction = reactionModel;

			Name = Reaction.Name.ToString();

			var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			FilePath = Reaction.WorkFolder?.FullName ?? "<no setting>";

			IsInactive = false == Reaction.IsEnable;

			IsInvalid = false == Reaction.IsValid;

			IsSelected = false;
		}



		private DelegateCommand _OpenReactionCommand;
		public DelegateCommand OpenReactionCommand
		{
			get
			{
				return _OpenReactionCommand
					?? (_OpenReactionCommand = new DelegateCommand(() =>
					{
						PageVM.ShowReaction(Reaction);
					}));
			}
		}

		private DelegateCommand _DeleteCommand;
		public DelegateCommand DeleteCommand
		{
			get
			{
				return _DeleteCommand
					?? (_DeleteCommand = new DelegateCommand(() =>
					{
						PageVM.DeleteFolderReactionFile(this.Reaction);
					}
					));
			}
		}


		private DelegateCommand _ExportCommand;
		public DelegateCommand ExportCommand
		{
			get
			{
				return _ExportCommand
					?? (_ExportCommand = new DelegateCommand(() =>
					{
						// SaveFileDialogで保存先パスを取得

						// ファイルコピー
						// 出力先のファイル名を取得
						var dialog = new SaveFileDialog();

						dialog.Title = "ReactiveFolder - select export application policy file";
						dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

						dialog.AddExtension = true;
						dialog.FileName = Reaction.Name;
						dialog.Filter = $"Json|*.json|All|*.*";

						var result = dialog.ShowDialog();


						if (result != null && ((bool)result) == true)
						{
							var destFilePath = dialog.FileName;

							try
							{
								FileSerializeHelper.Save(destFilePath, Reaction);
							}
							catch
							{
								System.Diagnostics.Debug.WriteLine("failed export Reaction File.");
								System.Diagnostics.Debug.WriteLine("  to :" + destFilePath);

								throw;
							}

							// TODO: エクスポート完了のトースト表示
							// 出力先フォルダをトーストから開けるとよりモアベター
						}
					}
					));
			}
		}
	}
}
