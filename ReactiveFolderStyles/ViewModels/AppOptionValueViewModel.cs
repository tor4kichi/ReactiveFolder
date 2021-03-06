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

namespace ReactiveFolderStyles.ViewModels
{
	public static class AppOptionValueViewModelHelper
	{
		public static IEnumerable<AppOptionValueViewModel> FromAppOptionInstance(this AppOptionInstance inst)
		{
			return inst.Values.Join(inst.OptionDeclaration.UserProperties,
					(x) => x.ValiableName,
					(y) => y.ValiableName,
					(x, y) => AppOptionValueViewModelHelper.CreateAppOptionValueVM(x, y)
					);
		}


		public static AppOptionValueViewModel CreateAppOptionValueVM(AppOptionValue val, AppOptionProperty prop)
		{
			if (prop is InputAppOptionProperty)
			{
				return new InputOptionValueViewModel(val, prop as InputAppOptionProperty);
			}
			else if (prop is FileOutputAppOptionProperty)
			{
				return new FileOutputOptionValueViewModel(val, prop as FileOutputAppOptionProperty);
			}
			else if (prop is FolderOutputAppOptionProperty)
			{
				return new OutputOptionValueViewModel(val, prop as FolderOutputAppOptionProperty);
			}
			else if (prop is StringListOptionProperty)
			{
				return new StringListOptionValueViewModel(val, prop as StringListOptionProperty);
			}
			else if (prop is RangeNumberAppOptionProperty)
			{
				return new RangeNumberOptionValueViewModel(val, prop as RangeNumberAppOptionProperty);
			}
			else if (prop is LimitedNumberAppOptionProerty)
			{
				return new LimitedNumberOptionValueViewModel(val, prop as LimitedNumberAppOptionProerty);
			}
			else if (prop is NumberAppOptionProperty)
			{
				return new NumberOptionValueViewModel(val, prop as NumberAppOptionProperty);
			}
			else
			{
				throw new NotSupportedException("cant create AppOptionValueViewModel.");
			}
		}
	}

	abstract public class AppOptionValueViewModel : BindableBase
	{
		public AppOptionValue OptionValue { get; private set; }
		public AppOptionProperty OptionProperty { get; private set; }

		public string PropertyName { get; private set; }

		public AppOptionValueViewModel(AppOptionValue val, AppOptionProperty property)
		{
			OptionValue = val;
			OptionProperty = property;

			PropertyName = OptionProperty.ValiableName;
		}
	}


	abstract public class TemplatedAppOptionValueViewModel<T> : AppOptionValueViewModel
		where T : AppOptionProperty
	{
		public T TemplateProperty { get; private set; }

		public TemplatedAppOptionValueViewModel(AppOptionValue val, T templatedProperty)
			: base(val, templatedProperty)
		{
			TemplateProperty = templatedProperty;
		}
	}


	public class InputOptionValueViewModel : TemplatedAppOptionValueViewModel<InputAppOptionProperty>
	{
		public InputOptionValueViewModel(AppOptionValue val, InputAppOptionProperty property)
			: base(val, property)
		{

		}
	}

	public class OutputOptionValueViewModel : TemplatedAppOptionValueViewModel<FolderOutputAppOptionProperty>
	{
		public OutputOptionValueViewModel(AppOptionValue val, FolderOutputAppOptionProperty property)
			: base(val, property)
		{

		}
	}

	public class FileOutputOptionValueViewModel : TemplatedAppOptionValueViewModel<FileOutputAppOptionProperty>
	{
		public string Extention { get; private set; }

		public FileOutputOptionValueViewModel(AppOptionValue val, FileOutputAppOptionProperty property)
			: base(val, property)
		{
			Extention = TemplateProperty.Extention;
		}
	}


	public class StringListOptionValueViewModel : TemplatedAppOptionValueViewModel<StringListOptionProperty>
	{

		public List<string> List { get; private set; }

		public ReactiveProperty<int> SelectedValue { get; private set; }

		public string SelectedItemValue
		{
			get
			{
				return TemplateProperty.List[SelectedValue.Value].Value;
			}
		}


		public StringListOptionValueViewModel(AppOptionValue val, StringListOptionProperty property)
			: base(val, property)
		{
			List = TemplateProperty.List.Select(x => x.Label).ToList();

			SelectedValue = new ReactiveProperty<int>((int)OptionValue.Value);
			SelectedValue.Subscribe(x =>
			{
				OptionValue.Value = x;
			});
		}
	}

	public class NumberOptionValueViewModel : TemplatedAppOptionValueViewModel<NumberAppOptionProperty>
	{
		public ReactiveProperty<string> NumberText { get; private set; }

		public NumberOptionValueViewModel(AppOptionValue val, NumberAppOptionProperty property)
			: base(val, property)
		{
			NumberText = new ReactiveProperty<string>(((int)val.Value).ToString());

			NumberText
				.Where(x =>
				{
					int temp;
					return int.TryParse(x, out temp);
				})
				.Select(x => int.Parse(x))
				.Subscribe(x => val.Value = x);

			NumberText.SetValidateNotifyError(x =>
			{
				if (String.IsNullOrWhiteSpace(x))
				{
					return "Input Number";
				}

				int temp;
				if (false == int.TryParse(x, out temp))
				{
					return "Number Only";
				}

				return null;
			});

		}
	}

	public class LimitedNumberOptionValueViewModel : TemplatedAppOptionValueViewModel<LimitedNumberAppOptionProerty>
	{
		public ReactiveProperty<string> NumberText { get; private set; }


		public LimitedNumberOptionValueViewModel(AppOptionValue val, LimitedNumberAppOptionProerty property)
			: base(val, property)
		{
			NumberText = new ReactiveProperty<string>(((int)val.Value).ToString());

			NumberText
				.Where(x =>
				{
					int temp;
					return int.TryParse(x, out temp);
				})
				.Select(x => int.Parse(x))
				.Subscribe(x => val.Value = x);

			NumberText.SetValidateNotifyError(x =>
			{
				if (String.IsNullOrWhiteSpace(x))
				{
					return "Input Number";
				}

				int temp;
				if (false == int.TryParse(x, out temp))
				{
					return "Number Only";
				}

				if (false == (TemplateProperty.MinValue <= temp && temp <= TemplateProperty.MaxValue))
				{
					return $"Number Out of Range";
				}

				return null;
			});

		}
	}

	public class RangeNumberOptionValueViewModel : TemplatedAppOptionValueViewModel<RangeNumberAppOptionProperty>
	{

		public ReactiveProperty<int> CurrentValue { get; private set; }

		public int MaxValue { get; private set; }
		public int MinValue { get; private set; }

		public int SkipAmount { get; private set; }

		public RangeNumberOptionValueViewModel(AppOptionValue val, RangeNumberAppOptionProperty property)
			: base(val, property)
		{
			CurrentValue = new ReactiveProperty<int>((int)val.Value);

			CurrentValue.Subscribe(x => val.Value = x);

			MaxValue = TemplateProperty.MaxValue;
			MinValue = TemplateProperty.MinValue;
			SkipAmount = TemplateProperty.SkipAmount;
		}
	}

}
