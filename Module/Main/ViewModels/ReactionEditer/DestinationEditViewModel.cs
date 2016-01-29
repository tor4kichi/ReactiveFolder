using System;
using System.Linq;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Model;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using ReactiveFolder.Model.Destinations;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public class DestinationEditViewModel : ReactionEditViewModelBase, IDisposable
	{
		public FolderReactionModel ReactionModel;

		protected CompositeDisposable _CompositeDisposable { get; private set; }

		public ReadOnlyReactiveProperty<DestinationViewModelBase> DestinationVM { get; private set; }

		public ReactiveProperty<bool> IsSameInputFolderChecked { get; set; }
		public ReactiveProperty<bool> IsInputChildFolderChecked { get; set; }
		public ReactiveProperty<bool> IsAbsoluteFolderChecked { get; set; }


		SameInputReactiveDestination _CachedSameInputDest;
		ChildReactiveDestination _CachedInputChildFolderDest;
		AbsolutePathReactiveDestination _CachedAbsoluteDest;

		public DestinationEditViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			ReactionModel = reactionModel;
			_CompositeDisposable = new CompositeDisposable();

			CacheDestinationModel();

			// 1. Checked系が変化すると
			IsSameInputFolderChecked = new ReactiveProperty<bool>(ReactionModel.Destination is SameInputReactiveDestination)
				.AddTo(_CompositeDisposable);
			IsInputChildFolderChecked = new ReactiveProperty<bool>(ReactionModel.Destination is ChildReactiveDestination)
				.AddTo(_CompositeDisposable);
			IsAbsoluteFolderChecked = new ReactiveProperty<bool>(ReactionModel.Destination is AbsolutePathReactiveDestination)
				.AddTo(_CompositeDisposable);


			// 2. CheckedのSubscriberによってReactionModel.Destinationが更新されて
			IsSameInputFolderChecked.Where(x => x)
				.Subscribe(_ =>
				{
					CacheDestinationModel();

					if (_CachedSameInputDest == null)
					{
						_CachedSameInputDest = new SameInputReactiveDestination();
					}

					ReactionModel.Destination = _CachedSameInputDest;
				})
				.AddTo(_CompositeDisposable);

			IsInputChildFolderChecked.Where(x => x)
				.Subscribe(_ =>
				{
					CacheDestinationModel();

					if (_CachedInputChildFolderDest == null)
					{
						_CachedInputChildFolderDest = new ChildReactiveDestination();
					}

					ReactionModel.Destination = _CachedInputChildFolderDest;
				})
				.AddTo(_CompositeDisposable);

			IsAbsoluteFolderChecked.Where(x => x)
				.Subscribe(_ =>
				{
					CacheDestinationModel();

					if (_CachedAbsoluteDest == null)
					{
						_CachedAbsoluteDest = new AbsolutePathReactiveDestination();
					}

					ReactionModel.Destination = _CachedAbsoluteDest;
				})
				.AddTo(_CompositeDisposable);


			// 3. ReactionModel.Destinationの更新に合わせてVMに変換
			DestinationVM = ReactionModel.ObserveProperty(x => x.Destination)
				.Select(ModelToVM)
				.ToReadOnlyReactiveProperty()
				.AddTo(_CompositeDisposable);


		}



		private void CacheDestinationModel()
		{
			if (ReactionModel.Destination is SameInputReactiveDestination)
			{
				_CachedSameInputDest = ReactionModel.Destination as SameInputReactiveDestination;
			}
			else if (ReactionModel.Destination is ChildReactiveDestination)
			{
				_CachedInputChildFolderDest = ReactionModel.Destination as ChildReactiveDestination;
			}
			else if (ReactionModel.Destination is AbsolutePathReactiveDestination)
			{
				_CachedAbsoluteDest = ReactionModel.Destination as AbsolutePathReactiveDestination;
			}
			else
			{
				throw new Exception();
			}
		}

		private DestinationViewModelBase ModelToVM(ReactiveDestinationBase destModel)
		{
			if (destModel is SameInputReactiveDestination)
			{
				return new SameInputDestinationViewModel(ReactionModel);
			}
			else if (destModel is ChildReactiveDestination)
			{
				return new InputChildFolderDestinationViewModel(ReactionModel);
			}
			else if (destModel is AbsolutePathReactiveDestination)
			{
				return new AbsolutePathDestinationViewModel(ReactionModel);
			}
			else
			{
				throw new Exception();
			}
		}

		public void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}

		protected override bool IsValidateModel()
		{
			return Reaction.ValidateDestination().IsValid;
		}
	}
}
