using System;
using System.Linq;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using ReactiveFolder.Models.Destinations;
using System.IO;
using Microsoft.Practices.Prism.Commands;
using ReactiveFolderStyles.Models;
using System.Collections.Generic;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class DestinationEditViewModel : ReactionEditViewModelBase
	{
		AbsolutePathReactiveDestination Destination;


		public string OutputFolderPath { get; private set; }

		public ReactiveProperty<string> OutputNamePattern { get; private set; }

		public ReadOnlyReactiveProperty<string> OutputPathSample { get; private set; }


		public ReactiveProperty<string> RenamePattern { get; private set; }

		public DestinationEditViewModel(PageManager pageManager, FolderReactionModel reactionModel)
			: base(pageManager, reactionModel)
		{
			_CompositeDisposable = new CompositeDisposable();

			Destination = reactionModel.Destination as AbsolutePathReactiveDestination;

			Reaction.ObserveProperty(x => x.IsDestinationValid)
				.Subscribe(x => IsValid.Value = x)
				.AddTo(_CompositeDisposable);

			OutputNamePattern = reactionModel.Destination.ToReactivePropertyAsSynchronized(x => x.OutputNamePattern)
				.AddTo(_CompositeDisposable);


			OutputFolderPath = Destination.AbsoluteFolderPath;

			OutputPathSample = Observable.Merge(
					Destination.ObserveProperty(x => x.AbsoluteFolderPath).Where(x => false == String.IsNullOrWhiteSpace(x)).ToUnit(),
					Destination.ObserveProperty(x => x.OutputNamePattern).Throttle(TimeSpan.FromSeconds(0.75)).ToUnit(),
					Reaction.ObserveProperty(x => x.OutputType).ToUnit()
				)
				.Select(_ => Destination.TestRename())
				.Where(x => false == String.IsNullOrEmpty(x))
				.Select(x =>
				{
					if (Reaction.OutputType == ReactiveFolder.Models.Util.FolderItemType.Folder)
					{
						return Path.Combine(Destination.AbsoluteFolderPath, x);
					}
					else
					{
						return Path.Combine(Destination.AbsoluteFolderPath, x) + ".extention";
					}
				})
				.ToReadOnlyReactiveProperty()
				.AddTo(_CompositeDisposable);



		}

		protected override IEnumerable<string> GetValidateError()
		{
			if (String.IsNullOrEmpty(Reaction.Destination.AbsoluteFolderPath))
			{
				yield return "Select output folder";
			}
			else if (false == Directory.Exists(Reaction.Destination.AbsoluteFolderPath))
			{
				yield return "Missing selected output folder";
			}
		}


		private DelegateCommand _SelectOutputFolderCommand;
		public DelegateCommand SelectOutputFolderCommand
		{
			get
			{
				return _SelectOutputFolderCommand
					?? (_SelectOutputFolderCommand = new DelegateCommand(() =>
					{
						// 出力先の絶対パスをFolder選択ダイアログを使って取得する
						var dialog = new WPFFolderBrowser.WPFFolderBrowserDialog("Select output folder.");

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


								// Note: Destination.AbsoluteFolderPath が変更されるとReactionのファイル更新情報がセーブを待たずに直ちにリセットされます。
								Destination.AbsoluteFolderPath = folderInfo.FullName;

								OutputFolderPath = folderInfo.FullName;
								OnPropertyChanged(nameof(OutputFolderPath));
							}
						}
						finally
						{
							//							dialog.Dispose();
							//							ofp.Dispose();
						}



					}));
			}
		}



		private DelegateCommand _ResetRenamePartternCommand;
		public DelegateCommand ResetRenamePartternCommand
		{
			get
			{
				return _ResetRenamePartternCommand
					?? (_ResetRenamePartternCommand = new DelegateCommand(() =>
					{
						RenamePattern.Value = ReactiveDestinationBase.DefaultRenamePattern;

					}));
			}
		}

		
	}
}
