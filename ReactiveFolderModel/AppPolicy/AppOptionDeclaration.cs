using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.AppPolicy
{
	abstract public class AppOptionDeclarationBase : BindableBase
	{
		[DataMember]
		private string _Name;

		public string Name
		{
			get
			{
				return _Name;
			}
			set
			{
				SetProperty(ref _Name, value);
			}
		}

		[DataMember]
		private string _OptionTextPattern;

		public string OptionTextPattern
		{
			get
			{
				return _OptionTextPattern;
			}
			set
			{
				SetProperty(ref _OptionTextPattern, value);
			}
		}



		
		[DataMember]
		public int Id { get; private set; }

		[DataMember]
		private int _Order;

		public int Order
		{
			get
			{
				return _Order;
			}
			set
			{
				SetProperty(ref _Order, value);
			}
		}


		[DataMember]
		protected ObservableCollection<AppOptionProperty> _UserProperties;

		public ReadOnlyObservableCollection<AppOptionProperty> UserProperties { get; private set; }





		private AppOptionValueSet _CurrentOptionValueSet;
		public AppOptionValueSet CurrentOptionValueSet
		{
			get
			{
				return _CurrentOptionValueSet;
			}
		}


		public void UpdateOptionValueSet(AppOptionValueSet optionValueSet)
		{
			_CurrentOptionValueSet = optionValueSet;
		}



		public AppOptionDeclarationBase(int id)
		{
			Name = "";
			Id = id;
			Order = 0;
			_UserProperties = new ObservableCollection<AppOptionProperty>();
			UserProperties = new ReadOnlyObservableCollection<AppOptionProperty>(_UserProperties);
		}

		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			UserProperties = new ReadOnlyObservableCollection<AppOptionProperty>(_UserProperties);
		}

	

		public bool Validate()
		{
			// TODO: CurrentOptionValueSetのValidate


			if (String.IsNullOrWhiteSpace(OptionTextPattern))
			{
				return false;
			}

			// UserPropertyの全てをValidateチェック
			foreach (var prop in UserProperties)
			{
				if (false == prop.Validate())
				{
					return false;
				}
			}

			// OptionTextPatternに全てのUserProperties.ValiableNameが含まれているか
			foreach (var prop in UserProperties)
			{
				var patternedValiableName = ToPatternText(prop);

				if (false == this.OptionTextPattern.Contains(patternedValiableName))
				{
					return false;
				}
			}

			return true;
		}

		public bool CanGenerateOptionText()
		{
			if (CurrentOptionValueSet == null)
			{
				return false;
			}

			if (CurrentOptionValueSet.OptionId != this.Id)
			{
				return false;
			}

			if (_UserProperties.Count != CurrentOptionValueSet.Values.Count())
			{
				return false;
			}

			for (int propertyItr = 0; propertyItr < _UserProperties.Count; ++propertyItr)
			{
				var prop = _UserProperties[propertyItr];
				var val = CurrentOptionValueSet.Values[propertyItr];

				if (prop.CanConvertOptionText(val))
				{
					return false;
				}
			}

			return true;
		}


		public string GenerateOptionText()
		{
			if (_UserProperties.Count != CurrentOptionValueSet.Values.Count())
			{
				throw new Exception();
			}

			var outputtext = OptionTextPattern;

			// UserPropertiesにvaluesを渡して、オプションテキストを生成する
			for (int propertyItr = 0; propertyItr < _UserProperties.Count; ++propertyItr)
			{
				var prop = _UserProperties[propertyItr];
				var val = CurrentOptionValueSet.Values[propertyItr];

				var patternedValiableName = ToPatternText(prop);

				if (OptionTextPattern.Contains(patternedValiableName))
				{
					var convertedOptionText = prop.ConvertOptionText(val);

					outputtext = outputtext.Replace(patternedValiableName, convertedOptionText);
				}
			}

			return outputtext;
		}

		protected string ToPatternText(AppOptionProperty prop)
		{
			return $"%{prop.ValiableName}%";
		}



		public AppOptionValueSet CreateValueSet()
		{
			return new AppOptionValueSet()
			{
				OptionId = this.Id,
				Values = _UserProperties
					.Select(x =>
						new AppOptionValue()
						{
							Name = x.ValiableName,
							Value = x.DefaultValue
						}
					)
					.ToArray()
			};
		}


		
	}

	[DataContract]
	public class AppInputOptionDeclaration : AppOptionDeclarationBase
	{
		private InputAppOptionProperty InputProperty { get; set; }


		public AppInputOptionDeclaration(int id)
			: base(id)
		{
			InputProperty = new InputAppOptionProperty();

			_UserProperties.Add(InputProperty);

			OptionTextPattern = ToPatternText(InputProperty);
		}
	}

	[DataContract]
	public class AppOutputOptionDeclaration : AppOptionDeclarationBase
	{
		private OutputAppOptionProperty OutputProperty { get; set; }

		public AppOutputOptionDeclaration(int id)
			: base(id)
		{
			OutputProperty = new OutputAppOptionProperty();

			_UserProperties.Add(OutputProperty);

			OptionTextPattern = ToPatternText(OutputProperty);
		}
	}

	[DataContract]
	public class AppOptionDeclaration : AppOptionDeclarationBase
	{


		public AppOptionDeclaration(int id)
			: base(id)
		{

			
		}

		

		protected void AddProperty(AppOptionProperty property)
		{
			_UserProperties.Add(property);
		}

		protected bool RemoveProperty(AppOptionProperty property)
		{
			return _UserProperties.Remove(property);
		}
	}







	[DataContract]
	abstract public class AppOptionProperty : BindableBase
	{
		private string _ValiableName;

		[DataMember]
		public string ValiableName
		{
			get
			{
				return _ValiableName;
			}
			set
			{
				SetProperty(ref _ValiableName, value);
			}
		}

		abstract public dynamic DefaultValue { get; }

		abstract public bool Validate();
		abstract public bool CanConvertOptionText(dynamic value);
		abstract public string ConvertOptionText(dynamic value = null);
	}


	[DataContract]
	public class InputAppOptionProperty : AppOptionProperty
	{
		public override dynamic DefaultValue { get { return ""; } }


		public override bool Validate()
		{
			return true;
		}

		public override bool CanConvertOptionText(dynamic value)
		{
			return value is string;
		}

		public override string ConvertOptionText(dynamic value = null)
		{
			return value as string;
		}
	}


	[DataContract]
	public class OutputAppOptionProperty : AppOptionProperty
	{
		public override dynamic DefaultValue { get { return ""; } }

		public override bool Validate()
		{
			return true;
		}

		public override bool CanConvertOptionText(dynamic value)
		{
			return value is string;
		}

		public override string ConvertOptionText(dynamic value = null)
		{
			return value as string;
		}
	}



	[DataContract]
	public class StringListOptionProperty : AppOptionProperty
	{
		public struct StringListItem
		{
			public string Label { get; set; }
			public string Value { get; set; }
		}


		[DataMember]
		private ObservableCollection<StringListItem> _List;

		public ReadOnlyObservableCollection<StringListItem> List { get; private set; }

		[DataMember]
		public int DefaultIndex { get; set; }



		public StringListOptionProperty()
		{
			_List = new ObservableCollection<StringListItem>();

			List = new ReadOnlyObservableCollection<StringListItem>(_List);
		}

		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			List = new ReadOnlyObservableCollection<StringListItem>(_List);
		}





		public override dynamic DefaultValue { get { return DefaultIndex; } }




		public override bool Validate()
		{
			if (_List.Count == 0)
			{
				return false;
			}

			return true;
		}

		public override bool CanConvertOptionText(dynamic value = null)
		{
			if (value == null)
			{
				return true;
			}


			if (value is int)
			{
				var index = (int)value;

				// index is inside List
				return 0 <= index && index < _List.Count;
			}
			else
			{
				return false;
			}
		}


		public override string ConvertOptionText(dynamic value = null)
		{
			if (value == null)
			{
				return _List[DefaultIndex].Value;
			}

			var index = (int)value;
			return _List[index].Value;
		}


		public void AddItem(string label, string val)
		{
			_List.Add(new StringListItem()
				{
					Label = label,
					Value = val
				}
			);
		}

		public void RemoveItem(int index)
		{
			_List.Remove(_List[index]);
		}

		public bool RemoveItem(StringListItem item)
		{
			return _List.Remove(item);
		}

	}

	[DataContract]
	public class NumberAppOptionProperty : AppOptionProperty
	{
		public override dynamic DefaultValue
		{
			get
			{
				return 0;
			}
		}


		public override bool Validate()
		{
			return true;
		}

		public override bool CanConvertOptionText(dynamic value)
		{
			if (false == value is int)
			{
				return false;
			}

			return true;
		}

		public override string ConvertOptionText(dynamic value = null)
		{
			int castVal = (int)value;

			return castVal.ToString();
		}


	}

	[DataContract]
	public class RangeNumberAppOptionProperty : NumberAppOptionProperty
	{
		[DataMember]
		private int _MinValue;
		public int MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				SetProperty(ref _MinValue, value);
			}
		}

		[DataMember]
		private int _MaxValue;
		public int MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				SetProperty(ref _MaxValue, value);
			}
		}



		public RangeNumberAppOptionProperty()
		{
			_MinValue = 0;
			_MaxValue = 100;
		}



		

		public override bool CanConvertOptionText(dynamic value)
		{
			if (false == value is int)
			{
				return false;
			}

			int castVal = (int)value;

			return MinValue <= castVal && castVal <= MaxValue;
		}

		
	}
}
