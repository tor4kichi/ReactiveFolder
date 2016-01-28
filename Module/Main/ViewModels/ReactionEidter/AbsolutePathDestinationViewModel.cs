using Microsoft.Practices.Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using ReactiveFolder.Model.Destinations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class AbsolutePathDestinationViewModel : DestinationViewModelBase
	{
		AbsolutePathReactiveDestination Destination;

		public string AbsoluteFolderPath { get; private set; }

		public ReadOnlyReactiveProperty<string> OutputPathSample { get; private set; }

		public AbsolutePathDestinationViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			Destination = reactionModel.Destination as AbsolutePathReactiveDestination;

			AbsoluteFolderPath = Destination.AbsoluteFolderPath;

			OutputPathSample = Observable.CombineLatest(
					Destination.ObserveProperty(x => x.AbsoluteFolderPath)
						.Where(x => false == String.IsNullOrWhiteSpace(x))
					, OutputNamePattern
				)
				.Select(x => {
					return Path.Combine(x[0], x[1]);
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
								AbsoluteFolderPath = folderInfo.FullName;
								OnPropertyChanged(nameof(AbsoluteFolderPath));
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
	}
}
