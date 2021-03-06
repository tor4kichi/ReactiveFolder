﻿using Microsoft.Practices.Prism.Mvvm;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolderStyles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	public enum ValidationState
	{
		WaitFirstValidate,

		Valid,
		Invalid
	}

	abstract public class ReactionEditViewModelBase : BindableBase, IDisposable
	{
		public PageManager PageManager { get; private set; }
		public FolderReactionModel Reaction { get; private set; }

		public ReactiveProperty<ValidationState> ValidateState { get; private set; }
		public ReactiveProperty<bool> IsValid { get; private set; }

		protected CompositeDisposable _CompositeDisposable;

		public ReactionEditViewModelBase(PageManager pageManager, FolderReactionModel reactionModel)
		{
			PageManager = pageManager;
			Reaction = reactionModel;
			_CompositeDisposable = new CompositeDisposable();


			IsValid = new ReactiveProperty<bool>(false)
				.AddTo(_CompositeDisposable);

			ValidateState = IsValid
				.Select(x => x ? ValidationState.Valid : ValidationState.Invalid)
				.ToReactiveProperty(ValidationState.WaitFirstValidate)
				.AddTo(_CompositeDisposable);
		}

		public virtual void Dispose()
		{
			_CompositeDisposable?.Dispose();
			_CompositeDisposable = null;
		}

		protected virtual IEnumerable<string> GetValidateError()
		{
			return Enumerable.Empty<string>();
		}


		private DelegateCommand _ShowValidateErrorCommand;
		public DelegateCommand ShowValidateErrorCommand
		{
			get
			{
				return _ShowValidateErrorCommand
					?? (_ShowValidateErrorCommand = new DelegateCommand(() => 
					{

						var errors = GetValidateError();

						foreach (var err in errors)
						{
							PageManager.ShowError(err);
						}
					}));
			}
		}

	}
}
