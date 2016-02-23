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
	[DataContract]
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
							ValiableName = x.ValiableName,
							Value = x.DefaultValue
						}
					)
					.ToArray()
			};
		}
	}
	

	[DataContract]
	public class AppIOPathOptionDeclaration : AppOptionDeclarationBase
	{
		private IOPathAppOptionProperty IOPathProperty { get; set; }

		public AppIOPathOptionDeclaration(string name, int id)
			: base(id)
		{
			IOPathProperty = new IOPathAppOptionProperty(name);

			_UserProperties.Add(IOPathProperty);

			OptionTextPattern = ToPatternText(IOPathProperty);
		}


		[OnDeserialized]
		public new void SetValuesOnDeserialized(StreamingContext context)
		{
			IOPathProperty = _UserProperties[0] as IOPathAppOptionProperty;
		}

	}





	[DataContract]
	public class AppOptionDeclaration : AppOptionDeclarationBase
	{


		public AppOptionDeclaration(int id)
			: base(id)
		{

			
		}

		

		public void AddProperty(AppOptionProperty property)
		{
			_UserProperties.Add(property);
		}

		public bool RemoveProperty(AppOptionProperty property)
		{
			return _UserProperties.Remove(property);
		}
	}







	
}
