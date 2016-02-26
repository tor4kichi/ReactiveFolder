using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Models.Util;
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
		protected ObservableCollection<AppOptionProperty> _UserProperties { get; set; }

		public ReadOnlyObservableCollection<AppOptionProperty> UserProperties { get; private set; }





		private AppOptionInstance _CurrentOptionValueSet;
		public AppOptionInstance CurrentOptionValueSet
		{
			get
			{
				return _CurrentOptionValueSet;
			}
		}


		public void UpdateOptionValueSet(AppOptionInstance optionValueSet)
		{
			_CurrentOptionValueSet = optionValueSet;
		}

		public AppOptionDeclarationBase()
		{

		}

		public AppOptionDeclarationBase(string name, int id)
		{
			Name = name;
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


		public virtual void Rollback(AppOptionDeclarationBase other)
		{
			this.Name = other.Name;
			this.Order = other.Order;
			this.OptionTextPattern = other.OptionTextPattern;

			
			var removePropties = this.UserProperties.ToArray();
			foreach (var remProp in removePropties)
			{
				this._UserProperties.Remove(remProp);
			}

			foreach (var addProp in other.UserProperties)
			{
				this._UserProperties.Add(addProp);
			}
			
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
					var convertedOptionText = prop.ConvertOptionText(val.Value);

					if (prop is PathAppOptionProperty)
					{
						convertedOptionText = $"\"{convertedOptionText}\"";
                    }

					outputtext = outputtext.Replace(patternedValiableName, convertedOptionText);
				}
			}

			return outputtext;
		}

		protected string ToPatternText(AppOptionProperty prop)
		{
			return $"%{prop.ValiableName}%";
		}



		public AppOptionInstance CreateInstance()
		{
			var values = _UserProperties
				.Select(x =>
					new AppOptionValue()
					{
						ValiableName = x.ValiableName,
						Value = x.DefaultValue
					}
				).ToArray();

			return new AppOptionInstance(this.Id, values, this);
		}
	}
	

	[DataContract]
	public class AppInputOptionDeclaration : AppOptionDeclarationBase
	{
		private InputAppOptionProperty InputPathProperty { get; set; }


		public AppInputOptionDeclaration()
		{ }

		public AppInputOptionDeclaration(string name, int id)
			: base(name, id)
		{
			InputPathProperty = new InputAppOptionProperty(name);

			_UserProperties.Add(InputPathProperty);

			OptionTextPattern = ToPatternText(InputPathProperty);
		}


		[OnDeserialized]
		public void OnDeserialized(StreamingContext context)
		{
			InputPathProperty = _UserProperties[0] as InputAppOptionProperty;
		}


		public override void Rollback(AppOptionDeclarationBase other)
		{
			base.Rollback(other);

			InputPathProperty = _UserProperties[0] as InputAppOptionProperty;

		}

	}

	[DataContract]
	public class AppOutputOptionDeclaration : AppOptionDeclaration
	{
		[DataMember]
		private FolderItemType _OutputType;
		public FolderItemType OutputType
		{
			get
			{
				return _OutputType;
			}
			set
			{
				if (SetProperty(ref _OutputType, value))
				{
					//
				}
			}
		}



		public AppOptionProperty OutputPathProperty { get; private set; }


		public AppOutputOptionDeclaration() { }

		public AppOutputOptionDeclaration(string name, int id, FolderItemType initialOutputType)
			: base(name, id)
		{
			OutputType = initialOutputType;
			ResetProperty(initialOutputType);
		}

		[OnDeserialized]
		public void OnDeserialized(StreamingContext context)
		{
			if (_UserProperties.Count > 0)
			{
				OutputPathProperty = _UserProperties[0];
			}
		}



		public override void Rollback(AppOptionDeclarationBase other)
		{
			base.Rollback(other);

			OutputPathProperty = UserProperties[0];
		}



		private void ResetProperty(FolderItemType type)
		{
			_UserProperties.Remove(OutputPathProperty);

			switch (type)
			{
				case FolderItemType.File:
					OutputPathProperty = new FileOutputAppOptionProperty(Name);
					break;
				case FolderItemType.Folder:
					OutputPathProperty = new FolderOutputAppOptionProperty(Name);
					break;
				default:
					break;
			}

			_UserProperties.Add(OutputPathProperty);

			OptionTextPattern = ToPatternText(OutputPathProperty);
		}



		public override void AddProperty(AppOptionProperty property)
		{
			if (property is FolderOutputAppOptionProperty)
			{
				throw new Exception();
			}

			base.AddProperty(property);
		}

		public override bool RemoveProperty(AppOptionProperty property)
		{
			if (property is FolderOutputAppOptionProperty)
			{
				throw new Exception();
			}

			return base.RemoveProperty(property);
		}
	}

	


	[DataContract]
	public class AppOptionDeclaration : AppOptionDeclarationBase
	{
		public AppOptionDeclaration() { }

		public AppOptionDeclaration(string name, int id)
			: base(name, id)
		{

			
		}

		

		public virtual void AddProperty(AppOptionProperty property)
		{
			_UserProperties.Add(property);
		}

		public virtual bool RemoveProperty(AppOptionProperty property)
		{
			return _UserProperties.Remove(property);
		}


		
	}







	
}
