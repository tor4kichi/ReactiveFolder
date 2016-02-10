﻿using System;
using System.Linq;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using ReactiveFolder.Model.Destinations;
using System.IO;
using Microsoft.Practices.Prism.Commands;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class DestinationEditViewModel : ReactionEditViewModelBase
	{
		AbsolutePathReactiveDestination Destination;


		public string OutputFolderPath { get; private set; }

		public ReactiveProperty<string> OutputNamePattern { get; private set; }

		public ReadOnlyReactiveProperty<string> OutputPathSample { get; private set; }


		public ReactiveProperty<string> RenamePattern { get; private set; }

		public DestinationEditViewModel(FolderReactionModel reactionModel)
			: base(@"Destination", reactionModel)
		{
			_CompositeDisposable = new CompositeDisposable();

			Destination = reactionModel.Destination as AbsolutePathReactiveDestination;

			Reaction.ObserveProperty(x => x.IsDestinationValid)
				.Subscribe(x => IsValid.Value = x)
				.AddTo(_CompositeDisposable);

			OutputNamePattern = reactionModel.Destination.ToReactivePropertyAsSynchronized(x => x.OutputNamePattern)
				.AddTo(_CompositeDisposable);


			OutputFolderPath = Destination.AbsoluteFolderPath;

			OutputPathSample = Observable.CombineLatest(
					Destination.ObserveProperty(x => x.AbsoluteFolderPath)
						.Where(x => false == String.IsNullOrWhiteSpace(x))
					, Destination.ObserveProperty(x => x.OutputNamePattern)
						.Throttle(TimeSpan.FromSeconds(0.75))
						.Select(Destination.TestRename)
						.Where(x => false == String.IsNullOrWhiteSpace(x))
				)
				.Select(x =>
				{
					return Path.Combine(x[0], x[1] + ".extention");
				})
				.ToReadOnlyReactiveProperty()
				.AddTo(_CompositeDisposable);



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
						// TODO: use const
						RenamePattern.Value = ReactiveDestinationBase.DefaultRenamePattern;

					}));
			}
		}
	}
}
