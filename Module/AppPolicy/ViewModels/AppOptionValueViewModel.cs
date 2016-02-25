using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using ReactiveFolder.Models.AppPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.AppPolicy.ViewModels
{
	// Valueを変更するためのプロパティ

	abstract public class OptionValueViewModel : BindableBase, IDisposable
	{
		public AppOptionProperty Property { get; private set; }
		public AppOptionValue Value { get; private set; }

		protected CompositeDisposable _CompositeDisposable;

		public string PropertyName { get; private set; }




		public OptionValueViewModel(AppOptionProperty prop, AppOptionValue val)
		{
			Property = prop;
			Value = val;

			_CompositeDisposable = new CompositeDisposable();

			PropertyName = prop.ValiableName;
		}

		public void Dispose()
		{
			_CompositeDisposable.Dispose();
		}
	}


	abstract public class TemplatedOptionValueViewModel<T> : OptionValueViewModel
		where T : AppOptionProperty
	{
		public T TemplateProperty { get; private set; }

		public TemplatedOptionValueViewModel(T prop, AppOptionValue val)
			: base(prop, val)
		{
			TemplateProperty = prop;
		}
	}

	public class IOPathOptionValueViewModel : TemplatedOptionValueViewModel<InputPathAppOptionProperty>
	{
		public IOPathOptionValueViewModel(InputPathAppOptionProperty prop, AppOptionValue val)
			: base(prop, val)
		{

		}
	}

	public class StringListOptionValueViewModel : TemplatedOptionValueViewModel<StringListOptionProperty>
	{
		public List<string> List { get; private set; }

		public ReactiveProperty<string> SelectedLabel { get; private set; }


		public StringListOptionValueViewModel(StringListOptionProperty prop, AppOptionValue val)
			: base(prop, val)
		{
			List = prop.List.Select(x => x.Label)
				.ToList();

			SelectedLabel = new ReactiveProperty<string>();

			SelectedLabel.Subscribe(x =>
			{
				// Listから選択されたアイテムのインデックスを
				var item = prop.List.Single(y => y.Label == x);

				var itemIndex = prop.List.IndexOf(item);

				val.Value = itemIndex;
			});

		}
	}



	public class NumberOptionValueViewModel : TemplatedOptionValueViewModel<NumberAppOptionProperty>
	{
		public ReactiveProperty<string> NumberText { get; private set; }



		public NumberOptionValueViewModel(NumberAppOptionProperty prop, AppOptionValue val)
			: base(prop, val)
		{
			NumberText = new ReactiveProperty<string>(prop.ConvertOptionText(val.Value), mode: ReactivePropertyMode.DistinctUntilChanged);

			NumberText
				.Where(CanParseToInt)
				.Select(x => int.Parse(x))
				.Where(x => prop.CanConvertOptionText(x))
				.Subscribe(x => val.Value = x);

			NumberText.SetValidateNotifyError(text =>
			{
				int temp;
				if (false == int.TryParse(text, out temp))
				{
					return "Number Only";
				}

				return null;
			});
		}

		static internal bool CanParseToInt(string text)
		{
			int temp;
			return int.TryParse(text, out temp);
		}
	}


	// TODO: LimitedNumberOptionValueViewModel



	public class RangeNumberOptionValueViewModel : TemplatedOptionValueViewModel<RangeNumberAppOptionProperty>
	{
		public ReactiveProperty<string> MinValueText { get; private set; }
		public ReactiveProperty<string> MaxValueText { get; private set; }
		public ReactiveProperty<string> SkipNumberText { get; private set; }


		public RangeNumberOptionValueViewModel(RangeNumberAppOptionProperty prop, AppOptionValue val)
			: base(prop, val)
		{
			Func<string, string> numberValidate = (x) =>
			{
				return NumberOptionValueViewModel.CanParseToInt(x) ? null : "Number Only";
			};

			MinValueText = new ReactiveProperty<string>(prop.MinValue.ToString(), mode: ReactivePropertyMode.DistinctUntilChanged);
			MinValueText.SetValidateNotifyError(numberValidate);
			MinValueText
				.Where(NumberOptionValueViewModel.CanParseToInt)
				.Select(x => int.Parse(x))
				.Subscribe(x => prop.MinValue = x);


			MaxValueText = new ReactiveProperty<string>(prop.MaxValue.ToString(), mode: ReactivePropertyMode.DistinctUntilChanged);
			MaxValueText.SetValidateNotifyError(numberValidate);
			MaxValueText
				.Where(NumberOptionValueViewModel.CanParseToInt)
				.Select(x => int.Parse(x))
				.Subscribe(x => prop.MaxValue = x);


			SkipNumberText = new ReactiveProperty<string>(prop.SkipAmount.ToString(), mode: ReactivePropertyMode.DistinctUntilChanged);
			SkipNumberText.SetValidateNotifyError(numberValidate);
			SkipNumberText
				.Where(NumberOptionValueViewModel.CanParseToInt)
				.Select(x => int.Parse(x))
				.Subscribe(x => prop.SkipAmount = x);
		}



		
	}
}
