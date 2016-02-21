using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.AppPolicy
{
	[DataContract]
	public class AppOptionDeclaration : BindableBase
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
		public int Id { get; private set; }



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
		private ObservableCollection<AppOptionProperty> _UserProperties;

		public ReadOnlyObservableCollection<AppOptionProperty> UserProperties { get; private set; }


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



		public AppOptionDeclaration(int id)
		{
			Id = id;
			Name = "";
			OptionTextPattern = "";
			_UserProperties = new ObservableCollection<AppOptionProperty>();
			UserProperties = new ReadOnlyObservableCollection<AppOptionProperty>(_UserProperties);
			Order = 0;
		}

		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			UserProperties = new ReadOnlyObservableCollection<AppOptionProperty>(_UserProperties);
		}



		public bool Validate()
		{
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

		public bool CanConvertOptionText(params dynamic[] values)
		{
			if (_UserProperties.Count != values.Count())
			{
				return false;
			}

			for(int propertyItr = 0; propertyItr < _UserProperties.Count; ++propertyItr)
			{
				var prop = _UserProperties[propertyItr];
				var val = values[propertyItr];

				if (prop.CanConvertOptionText(val))
				{
					return false;
				}
			}

			return true;
		}


		public string ConvertOptionText(params dynamic[] values)
		{
			if (_UserProperties.Count != values.Count())
			{
				throw new Exception();
			}

			var outputtext = OptionTextPattern;

			// UserPropertiesにvaluesを渡して、オプションテキストを生成する
			for (int propertyItr = 0; propertyItr < _UserProperties.Count; ++propertyItr)
			{
				var prop = _UserProperties[propertyItr];
				var val = values[propertyItr];

				var patternedValiableName = ToPatternText(prop);

				if (OptionTextPattern.Contains(patternedValiableName))
				{
					var convertedOptionText = prop.ConvertOptionText(val);

					outputtext = outputtext.Replace(patternedValiableName, convertedOptionText);
				}
			}

			return outputtext;
		}

		private string ToPatternText(AppOptionProperty prop)
		{
			return $"%{prop.ValiableName}%";
		}
	}


	public enum PropertyType
	{
		StringList,
		RangeNumber,
		Number,
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



		abstract public bool Validate();
		abstract public bool CanConvertOptionText(dynamic value);
		abstract public string ConvertOptionText(dynamic value = null);
	}


	[DataContract]
	public class StringListOptionProperty : AppOptionProperty
	{
		[DataMember]
		private ObservableCollection<KeyValuePair<string, string>> _List;

		public ReadOnlyObservableCollection<KeyValuePair<string, string>> List { get; private set; }

		[DataMember]
		public int DefaultIndex { get; set; }


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
			_List.Add(new KeyValuePair<string, string>(label, val));
		}

		public void RemoveItem(int index)
		{
			_List.Remove(_List[index]);
		}

		public bool RemoveItem(KeyValuePair<string, string> pair)
		{
			return _List.Remove(pair);
		}

	}

	// TODO: AppOptionPropertyRangeNumber
	// TODO: AppOptionPropertyNumber

}
