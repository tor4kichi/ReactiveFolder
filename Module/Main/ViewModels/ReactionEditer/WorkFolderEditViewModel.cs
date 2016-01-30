﻿using Microsoft.Practices.Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class WorkFolderEditViewModel : ReactionEditViewModelBase
	{
		public ReactiveProperty<string> WorkFolderPath { get; private set; }



		private ReactiveProperty<bool> _IsValid;
		public override ReactiveProperty<bool> IsValid
		{
			get
			{
				return _IsValid;
			}
		}


		public WorkFolderEditViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{

			_IsValid = Reaction.ObserveProperty(x => x.IsWorkFolderValid)
				.ToReactiveProperty();



			WorkFolderPath = Reaction.ObserveProperty(x => x.WorkFolder)
				.Select(x => x.FullName)
				.ToReactiveProperty();
		}




		private DelegateCommand _FolderSelectCommand;
		public DelegateCommand FolderSelectCommand
		{
			get
			{
				return _FolderSelectCommand
					?? (_FolderSelectCommand = new DelegateCommand(() =>
					{
						// モニター対象となるフォルダを取得する
						var dialog = new WPFFolderBrowser.WPFFolderBrowserDialog("Select monitor target folder.");

						dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


						try
						{
							var result = dialog.ShowDialog();
							if (result.HasValue && result.Value
								&& false == String.IsNullOrWhiteSpace(dialog.FileName))
							{
								var folderInfo = new DirectoryInfo(dialog.FileName);

								if (false == folderInfo.Exists)
								{
									return;
								}
								Reaction.WorkFolder = folderInfo;

							}
						}
						finally
						{
							//							dialog.Dispose();						
						}


					}));
			}
		}
	}
}
