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

		[DataMember]
		public int PropertyId { get; private set; }

		public AppOptionProperty(int id, string valiableName)
		{
			ValiableName = valiableName;
		}
		abstract public dynamic DefaultValue { get; }

		abstract public bool Validate();
		abstract public bool CanConvertOptionText(dynamic value);
		abstract public string ConvertOptionText(dynamic value = null);
	}

	[DataContract]
	abstract public class PathAppOptionProperty : AppOptionProperty
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

		public PathAppOptionProperty(int id, string name)
			: base(id, name)
		{
		}
	}



	[DataContract]
	public class InputAppOptionProperty : PathAppOptionProperty
	{
		public override dynamic DefaultValue { get { return ""; } }


		

		public InputAppOptionProperty(int id, string name)
			: base(id, name)
		{
		}

		

		public override string ConvertOptionText(dynamic value = null)
		{
			var str = (string)value;
			return str;
		}
	}

	[DataContract]
	public class FolderOutputAppOptionProperty : PathAppOptionProperty
	{
	

		public FolderOutputAppOptionProperty(int id, string name)
			: base(id, name)
		{
		}

		

		public override string ConvertOptionText(dynamic value = null)
		{
			var str = (string)value;

			// 入力がファイルパスなら拡張子を取り除いてフォルダパスに変換
			if (Path.HasExtension(str))
			{
				str = Path.GetFileNameWithoutExtension(str);
			}

			return str;
		}
	}

	[DataContract]
	public class FileOutputAppOptionProperty : PathAppOptionProperty
	{
		[DataMember]
		private string _Extention;
		public string Extention
		{
			get
			{
				return _Extention;
			}
			set
			{
				SetProperty(ref _Extention, value);
			}
		}


		public bool IsSameInputExtention
		{
			get
			{
				return String.IsNullOrWhiteSpace(Extention);
			}
		}
		

		
		public FileOutputAppOptionProperty(int id, string name)
			: base(id, name)
			
		{
			Extention = "";
		}

		
		public override string ConvertOptionText(dynamic value = null)
		{
			var str = (string)value;
			if (false == IsSameInputExtention)
			{
				str = Path.ChangeExtension(str, Extention);
			}

			return str;
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


		public StringListOptionProperty(int id, string valiableName)
			: base(id, valiableName)
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


		public NumberAppOptionProperty(int id, string valiableName)
			: base(id, valiableName)
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

		public LimitedNumberAppOptionProerty(int id, string valiableName)
			: base(id, valiableName)
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
		private int _SkipAmount;
		public int SkipAmount
		{
			get
			{
				return _SkipAmount;
			}
			set
			{
				SetProperty(ref _SkipAmount, value);
			}
		}


		public RangeNumberAppOptionProperty(int id, string valiableName)
			: base(id, valiableName)
		{
			SkipAmount = 1;
		}
	}
}
