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
	public class IOPathAppOptionProperty : AppOptionProperty
	{
		public override dynamic DefaultValue { get { return ""; } }


		public override bool Validate()
		{
			return true;
		}


		public IOPathAppOptionProperty(string name)
		{
			this.ValiableName = name;
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
	public class StringListItem : BindableBase
	{
		[DataMember]
		private string _Label;
		public string Label
		{
			get
			{
				return _Label;
			}
			set
			{
				SetProperty(ref _Label, value);
			}
		}

		[DataMember]
		private string _Value;
		public string Value
		{
			get
			{
				return _Value;
			}
			set
			{
				SetProperty(ref _Value, value);
			}
		}
	}

	[DataContract]
	public class StringListOptionProperty : AppOptionProperty
	{


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
				return DefaultNumber;
			}
		}

		[DataMember]
		public int DefaultNumber { get; set; }


		public NumberAppOptionProperty()
		{
			DefaultNumber = 0;
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
	public class LimitedNumberAppOptionProerty : NumberAppOptionProperty
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

		public LimitedNumberAppOptionProerty()
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

	[DataContract]
	public class RangeNumberAppOptionProperty : LimitedNumberAppOptionProerty
	{
		[DataMember]
		private int _SkipNumber;
		public int SkipNumber
		{
			get
			{
				return _SkipNumber;
			}
			set
			{
				SetProperty(ref _SkipNumber, value);
			}
		}


		public RangeNumberAppOptionProperty()
		{
			SkipNumber = 1;
		}
	}
}
