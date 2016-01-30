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
	public class DestinationEditViewModel : ReactionEditViewModelBase
	{
		public ReadOnlyReactiveProperty<DestinationViewModelBase> DestinationVM { get; private set; }


		private ReactiveProperty<bool> _IsValid;
		public override ReactiveProperty<bool> IsValid
		{
			get
			{
				return _IsValid;
			}
		}


		public ReactiveProperty<bool> IsSameInputFolderChecked { get; set; }
		public ReactiveProperty<bool> IsInputChildFolderChecked { get; set; }
		public ReactiveProperty<bool> IsAbsoluteFolderChecked { get; set; }


		SameInputReactiveDestination _CachedSameInputDest;
		ChildReactiveDestination _CachedInputChildFolderDest;
		AbsolutePathReactiveDestination _CachedAbsoluteDest;

		public DestinationEditViewModel(FolderReactionModel reactionModel)
			: base(reactionModel)
		{
			_CompositeDisposable = new CompositeDisposable();



			_IsValid = Reaction.ObserveProperty(x => x.IsDestinationValid)
				.ToReactiveProperty();



			CacheDestinationModel();

			// 1. Checked系が変化すると
			IsSameInputFolderChecked = new ReactiveProperty<bool>(Reaction.Destination is SameInputReactiveDestination)
				.AddTo(_CompositeDisposable);
			IsInputChildFolderChecked = new ReactiveProperty<bool>(Reaction.Destination is ChildReactiveDestination)
				.AddTo(_CompositeDisposable);
			IsAbsoluteFolderChecked = new ReactiveProperty<bool>(Reaction.Destination is AbsolutePathReactiveDestination)
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

					Reaction.Destination = _CachedSameInputDest;
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

					Reaction.Destination = _CachedInputChildFolderDest;
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

					Reaction.Destination = _CachedAbsoluteDest;
				})
				.AddTo(_CompositeDisposable);


			// 3. ReactionModel.Destinationの更新に合わせてVMに変換
			DestinationVM = Reaction.ObserveProperty(x => x.Destination)
				.Select(ModelToVM)
				.ToReadOnlyReactiveProperty()
				.AddTo(_CompositeDisposable);


		}



		private void CacheDestinationModel()
		{
			if (Reaction.Destination is SameInputReactiveDestination)
			{
				_CachedSameInputDest = Reaction.Destination as SameInputReactiveDestination;
			}
			else if (Reaction.Destination is ChildReactiveDestination)
			{
				_CachedInputChildFolderDest = Reaction.Destination as ChildReactiveDestination;
			}
			else if (Reaction.Destination is AbsolutePathReactiveDestination)
			{
				_CachedAbsoluteDest = Reaction.Destination as AbsolutePathReactiveDestination;
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
				return new SameInputDestinationViewModel(Reaction);
			}
			else if (destModel is ChildReactiveDestination)
			{
				return new InputChildFolderDestinationViewModel(Reaction);
			}
			else if (destModel is AbsolutePathReactiveDestination)
			{
				return new AbsolutePathDestinationViewModel(Reaction);
			}
			else
			{
				throw new Exception();
			}
		}
	}
}
