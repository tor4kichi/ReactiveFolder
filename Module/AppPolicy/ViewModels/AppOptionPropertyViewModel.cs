﻿using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.AppPolicy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.AppPolicy.ViewModels
{

	public static class AppOptionPropertyViewModelHelper
	{
		public static AppOptionPropertyViewModel ToAppOptionPropertyVM(this AppOptionProperty property, AppOptionDeclarationViewModel declVM)
		{
			if (property is IOPathAppOptionProperty)
			{
				return new IOPathAppOptionPropertyViewModel(declVM, property as IOPathAppOptionProperty);
			}
			else if (property is StringListOptionProperty)
			{
				return new StringListAppOptionPropertyViewModel(declVM, property as StringListOptionProperty);
			}
			else if (property is RangeNumberAppOptionProperty)
			{
				return new RangeNumberAppOptionPropertyViewModel(declVM, property as RangeNumberAppOptionProperty);
			}
			else if (property is LimitedNumberAppOptionProerty)
			{
				return new LimitedNumberAppOptionPropertyViewModel(declVM, property as LimitedNumberAppOptionProerty);
			}
			else if (property is NumberAppOptionProperty)
			{
				return new NumberAppOptionPropertyViewModel(declVM, property as NumberAppOptionProperty);
			}
			else
			{
				throw new NotSupportedException();
			}
		}
	}

	abstract public class AppOptionPropertyViewModel : BindableBase
	{
		public AppOptionDeclarationViewModel DeclarationVM { get; private set; }
		public AppOptionProperty Property { get; private set; }

		// ユーザー設定のプロパティは小文字に限定する？
		// 読みやすさは小文字のほうがいいかも？
		public ReactiveProperty<string> ValiableName { get; private set; }

		public AppOptionPropertyViewModel(AppOptionDeclarationViewModel declVM, AppOptionProperty property)
		{
			DeclarationVM = declVM;
			Property = property;

			ValiableName = Property.ToReactivePropertyAsSynchronized(x => x.ValiableName);
		}

		private DelegateCommand _RemovePropertyCommand;
		public DelegateCommand RemovePropertyCommand
		{
			get
			{
				return _RemovePropertyCommand
					?? (_RemovePropertyCommand = new DelegateCommand(() =>
					{
						DeclarationVM.RemoveProperty(Property);
					}
					,
					() => false == (Property is IOPathAppOptionProperty)
					));
			}
		}
	}

	abstract public class TemplatedAppOptionPropertyViewModel<T> : AppOptionPropertyViewModel
		where T : AppOptionProperty
	{
		public T TemplateProperty { get; private set; }

		public TemplatedAppOptionPropertyViewModel(AppOptionDeclarationViewModel declVM, T property)
			: base(declVM, property)
		{
			TemplateProperty = property;
		}
	}


	public class IOPathAppOptionPropertyViewModel : TemplatedAppOptionPropertyViewModel<IOPathAppOptionProperty>
	{
		public IOPathAppOptionPropertyViewModel(AppOptionDeclarationViewModel declVM, IOPathAppOptionProperty property)
			: base(declVM, property)
		{

		}
	}

	public class StringListAppOptionPropertyViewModel : TemplatedAppOptionPropertyViewModel<StringListOptionProperty>
	{
		public ReadOnlyReactiveCollection<StringListItemViewModel> StringList { get; private set; }


		public StringListAppOptionPropertyViewModel(AppOptionDeclarationViewModel declVM, StringListOptionProperty property)
			: base(declVM, property)
		{
			StringList = TemplateProperty.List.ToReadOnlyReactiveCollection(
				x => new StringListItemViewModel(TemplateProperty, x)
				);
		}

		private DelegateCommand _AddStringListItemCommand;
		public DelegateCommand AddStringListItemCommand
		{
			get
			{
				return _AddStringListItemCommand
					?? (_AddStringListItemCommand = new DelegateCommand(() =>
					{
						TemplateProperty.AddItem("label", "value");
					}));
			}
		}
	}

	public class StringListItemViewModel : BindableBase, IDisposable
	{
		public StringListOptionProperty StringList { get; private set; }

		public StringListItem ListItem { get; private set; }

		public ReactiveProperty<string> Label { get; private set; }

		public ReactiveProperty<string> Value { get; private set; }


		public StringListItemViewModel(StringListOptionProperty stringList, StringListItem listItem)
		{
			StringList = stringList;
			ListItem = listItem;

			Label = ListItem.ToReactivePropertyAsSynchronized(x => x.Label);
			Value = ListItem.ToReactivePropertyAsSynchronized(x => x.Value);
		}

		public void Dispose()
		{
			Label.Dispose();
			Value.Dispose();
		}



		private DelegateCommand _RemoveStringListItemCommand;
		public DelegateCommand RemoveStringListItemCommand
		{
			get
			{
				return _RemoveStringListItemCommand
					?? (_RemoveStringListItemCommand = new DelegateCommand(() => 
					{
						StringList.RemoveItem(ListItem);
					}));
			}
		}
	}

	public class NumberAppOptionPropertyViewModel : TemplatedAppOptionPropertyViewModel<NumberAppOptionProperty>
	{
		public ReactiveProperty<string> DefaultNumberText { get; private set; }



		public NumberAppOptionPropertyViewModel(AppOptionDeclarationViewModel declVM, NumberAppOptionProperty property)
			: base(declVM, property)
		{
			DefaultNumberText = new ReactiveProperty<string>(TemplateProperty.ConvertOptionText(TemplateProperty.DefaultValue));

			DefaultNumberText
				.Where(CanParseToInt)
				.Select(x => int.Parse(x))
				.Where(x => TemplateProperty.CanConvertOptionText(x))
				.Subscribe(x => TemplateProperty.DefaultNumber = x);

			DefaultNumberText.SetValidateNotifyError(NumberValidate);
		}


		static internal Func<string, string> NumberValidate = (x) =>
		{
			return CanParseToInt(x) ? null : "Number Only";
		};

		static internal bool CanParseToInt(string text)
		{
			int temp;
			return int.TryParse(text, out temp);
		}
	}

	public class LimitedNumberAppOptionPropertyViewModel : TemplatedAppOptionPropertyViewModel<LimitedNumberAppOptionProerty>
	{
		public ReactiveProperty<string> MaxValueText { get; private set; }

		public ReactiveProperty<string> MinValueText { get; private set; }

		public LimitedNumberAppOptionPropertyViewModel(AppOptionDeclarationViewModel declVM, LimitedNumberAppOptionProerty property)
			: base(declVM, property)
		{
			MaxValueText = new ReactiveProperty<string>(TemplateProperty.MaxValue.ToString());

			MaxValueText
				.Where(NumberAppOptionPropertyViewModel.CanParseToInt)
				.Select(x => int.Parse(x))
				.Where(x => TemplateProperty.MinValue < x)
				.Subscribe(x => TemplateProperty.MaxValue = x);

			MaxValueText.SetValidateNotifyError(x =>
			{
				var str = NumberAppOptionPropertyViewModel.NumberValidate(x);
				if (str != null)
				{
					return str;
				}

				var val = int.Parse(x);
				if (val <= TemplateProperty.MinValue)
				{
					return "Min < Max";
				}

				return null;
			});


			MinValueText = new ReactiveProperty<string>(TemplateProperty.MinValue.ToString());

			MinValueText
				.Where(NumberAppOptionPropertyViewModel.CanParseToInt)
				.Select(x => int.Parse(x))
				.Where(x => TemplateProperty.MaxValue > x)
				.Subscribe(x => TemplateProperty.MinValue = x);

			MinValueText.SetValidateNotifyError(x =>
			{
				var str = NumberAppOptionPropertyViewModel.NumberValidate(x);
				if (str != null)
				{
					return str;
				}

				var val = int.Parse(x);
				if (TemplateProperty.MaxValue <= val)
				{
					return "Min < Max";
				}

				return null;
			});
		}
	}

	public class RangeNumberAppOptionPropertyViewModel : TemplatedAppOptionPropertyViewModel<RangeNumberAppOptionProperty>
	{
		public ReactiveProperty<string> SkipNumberText { get; private set; }

		public RangeNumberAppOptionPropertyViewModel(AppOptionDeclarationViewModel declVM, RangeNumberAppOptionProperty property)
			: base(declVM, property)
		{
			SkipNumberText = new ReactiveProperty<string>(TemplateProperty.SkipNumber.ToString());

			SkipNumberText
				.Where(NumberAppOptionPropertyViewModel.CanParseToInt)
				.Select(x => int.Parse(x))
				.Where(x => 0 < x)
				.Subscribe(x => TemplateProperty.SkipNumber = x);

			SkipNumberText.SetValidateNotifyError(x =>
			{
				var str = NumberAppOptionPropertyViewModel.NumberValidate(x);
				if (str != null)
				{
					return str;
				}

				var val = int.Parse(x);
				if (val <= 0)
				{
					return "input greater than One.";
				}

				return null;
			});
		}
	}
}
