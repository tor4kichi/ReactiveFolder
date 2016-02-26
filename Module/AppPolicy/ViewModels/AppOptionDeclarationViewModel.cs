using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models.AppPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Modules.AppPolicy.ViewModels
{
	public class AppOptionDeclarationViewModel : BindableBase, IDisposable
	{
		public ApplicationPolicyViewModel AppPolicyVM { get; private set; }
		public AppOptionDeclarationBase Declaration { get; private set; }

		public ReactiveProperty<string> Name { get; private set; }

		public ReactiveProperty<string> OptionTextPattern { get; private set; }

		public ReactiveProperty<string> Order { get; private set; }

		public ReadOnlyReactiveCollection<AppOptionPropertyViewModel> Properties { get; private set; }


		private CompositeDisposable _CompositeDisposable;

		// Propertiesを反映させたOptionTextPartternの表示サンプル
		public ReactiveProperty<string> SampleOptionText { get; private set; }

		public List<AddablePropertyListItem> AddableProperties { get; private set; }

		public bool IsDisplayOptionTextPattern { get; private set; }
		public bool IsDisplayProperty { get; private set; }

		public AppOptionDeclarationViewModel(ApplicationPolicyViewModel appPolicyVM, AppOptionDeclarationBase decl)
		{
			AppPolicyVM = appPolicyVM;
			Declaration = decl;

			_CompositeDisposable = new CompositeDisposable();

			Name = Declaration.ToReactivePropertyAsSynchronized(x => x.Name)
				.AddTo(_CompositeDisposable);

			OptionTextPattern = Declaration.ToReactivePropertyAsSynchronized(x => x.OptionTextPattern)
				.AddTo(_CompositeDisposable);

			Order = Declaration.ToReactivePropertyAsSynchronized(x => x.Order,
				convert: (model) => model.ToString(),
				convertBack: (vm) => int.Parse(vm),
				ignoreValidationErrorValue: true
			)
				.AddTo(_CompositeDisposable);

			Order.SetValidateNotifyError(x => {
				if (String.IsNullOrWhiteSpace(x))
				{
					return "Input Number";
				}
				int temp;
				return int.TryParse(x, out temp) ? null : "Number Only";
			});



			Properties = Declaration.UserProperties.ToReadOnlyReactiveCollection(
				x => x.ToAppOptionPropertyVM(this)
				)
				.AddTo(_CompositeDisposable);

			// TODO: OptionTextPartternとPropertiesの個数またはの内部パラメータの変更を検知して更新
			//SampleOptionText = 
			var addablePropTypes = Enum.GetValues(typeof(AddableAppOptionPropertyType)) as IEnumerable<AddableAppOptionPropertyType>;


			AddableProperties = addablePropTypes
				.Select(x => new AddablePropertyListItem(this, x.ToString(), x))
				.ToList();





			var isInputOption = decl is AppInputOptionDeclaration;
			IsDisplayOptionTextPattern = !isInputOption;
			IsDisplayProperty = !isInputOption;
		}

		private DelegateCommand _EditDeclarationCommand;
		public DelegateCommand EditDeclarationCommand
		{
			get
			{
				return _EditDeclarationCommand
					?? (_EditDeclarationCommand = new DelegateCommand(() =>
					{
						AppPolicyVM.OpenOptionDeclarationEditDialog(this.Declaration);
					}));
			}
		}


		private DelegateCommand _RemoveDeclarationCommand;
		public DelegateCommand RemoveDeclarationCommand
		{
			get
			{
				return _RemoveDeclarationCommand
					?? (_RemoveDeclarationCommand = new DelegateCommand(() =>
					{
						AppPolicyVM.RemoveDeclaration(this.Declaration);
					}
					, 
					() => false == (Declaration is AppInputOptionDeclaration)
					));
			}
		}






		public void Dispose()
		{
			_CompositeDisposable.Dispose();
		}

		internal void RemoveProperty(AppOptionProperty property)
		{
			if (Declaration is AppOptionDeclaration)
			{
				(Declaration as AppOptionDeclaration)
					.RemoveProperty(property);
			}
		}

		internal void AddProperty(AddableAppOptionPropertyType propertyType)
		{
			if (Declaration is AppOptionDeclaration)
			{
				(Declaration as AppOptionDeclaration)
					.AddProperty(propertyType.ToOptionProperty("name"));
			}
		}
	}

	public class AddablePropertyListItem : BindableBase
	{
		public AppOptionDeclarationViewModel OptionDeclVM { get; private set; }

		public string PropertyTypeLabel { get; private set; }

		public AddableAppOptionPropertyType PropertyType { get; private set; }


		public AddablePropertyListItem(AppOptionDeclarationViewModel optionDeclVM, string label, AddableAppOptionPropertyType type)
		{
			OptionDeclVM = optionDeclVM;
			PropertyTypeLabel = label;
			PropertyType = type;
		}

		

		private DelegateCommand _AddPropertyCommand;
		public DelegateCommand AddPropertyCommand
		{
			get
			{
				return _AddPropertyCommand
					?? (_AddPropertyCommand = new DelegateCommand(() =>
					{
						OptionDeclVM.AddProperty(PropertyType);
					}
					));
			}
		}
	}

	public enum AddableAppOptionPropertyType
	{
		StringList,
		Number,
		LimitedNumber,
		RangeNumber,
	}

	public static class AddableAppOptionPropertyTypeHelper
	{
		public static AppOptionProperty ToOptionProperty(this AddableAppOptionPropertyType propType, string valiableName)
		{
			switch (propType)
			{
				case AddableAppOptionPropertyType.StringList:
					return new StringListOptionProperty(valiableName);
				case AddableAppOptionPropertyType.Number:
					return new NumberAppOptionProperty(valiableName);
				case AddableAppOptionPropertyType.LimitedNumber:
					return new LimitedNumberAppOptionProerty(valiableName);
				case AddableAppOptionPropertyType.RangeNumber:
					return new RangeNumberAppOptionProperty(valiableName);
				default:
					throw new NotSupportedException();
			}
		}
	}


}
