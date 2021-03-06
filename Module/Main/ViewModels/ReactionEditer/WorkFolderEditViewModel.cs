﻿using Microsoft.Practices.Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Timings;
using ReactiveFolderStyles.Models;
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



	


		public WorkFolderEditViewModel(PageManager pageManager, FolderReactionModel reactionModel)
			: base(pageManager, reactionModel)
		{

			Reaction.ObserveProperty(x => x.IsWorkFolderValid)
				.Subscribe(x => IsValid.Value = x)
				.AddTo(_CompositeDisposable);


			WorkFolderPath = Reaction.ObserveProperty(x => x.WorkFolder)
				.Select(x => x?.FullName ?? "<not selected>")
				.ToReactiveProperty()
				.AddTo(_CompositeDisposable);

		}


		protected override IEnumerable<string> GetValidateError()
		{
			if (String.IsNullOrEmpty(Reaction.WorkFolderPath))
			{
				yield return "Select Target Folder";
			}
			else if (false == Directory.Exists(Reaction.WorkFolderPath))
			{
				yield return "Not found selected Target Folder";
			}
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

						var oldFolder = Reaction.WorkFolder;
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

								// Note: WorkFolderが変更されると、これまで処理したアイテムの内容がセーブを待たずに直ちにリセットされます。

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
