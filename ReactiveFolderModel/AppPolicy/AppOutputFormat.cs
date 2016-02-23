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
	public class AppOutputFormat : BindableBase
	{
		public const int IgnoreArgumentId = -1;


		[DataMember]
		private int _Id;
		public int Id
		{
			get { return _Id; }
		}

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
		private string _OutputExtention;
		public string OutputExtention
		{
			get
			{
				return _OutputExtention;
			}
			set
			{
				SetProperty(ref _OutputExtention, value);
			}
		}


		[DataMember]
		private ObservableCollection<AppOptionValueSet> _OptionValueSets { get; set; }
		public ReadOnlyObservableCollection<AppOptionValueSet> OptionValueSets { get; private set; }


		public AppOutputFormat(int id)
		{
			_Id = id;
			Name = "";
			OutputExtention = "";

			_OptionValueSets = new ObservableCollection<AppOptionValueSet>();
			OptionValueSets = new ReadOnlyObservableCollection<AppOptionValueSet>(_OptionValueSets);
		}

		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			OptionValueSets = new ReadOnlyObservableCollection<AppOptionValueSet>(_OptionValueSets);
		}



		public AppOptionValueSet AddOption(AppOptionDeclarationBase optionDecl)
		{
			var valueSet = _OptionValueSets.SingleOrDefault(x => x.OptionId == optionDecl.Id);

			if (valueSet == null)
			{
				valueSet = optionDecl.CreateValueSet();
				_OptionValueSets.Add(valueSet);
			}

			return valueSet;
		}


		public void RemoveOption(AppOptionDeclaration optionDecl)
		{
			var val = _OptionValueSets.SingleOrDefault(x => x.OptionId == optionDecl.Id);

			if (val != null)
			{
				_OptionValueSets.Remove(val);
			}
		}

		public void UpdateOption(AppOptionDeclarationBase decl, AppOptionValue[] values)
		{
			var val = _OptionValueSets.SingleOrDefault(x => x.OptionId == decl.Id);

			if (val.Values.Length != values.Length)
			{
				throw new Exception();
			}

			val.Values = values;

			for (int itr = 0; itr < val.Values.Length; ++itr)
			{
				val.Values[itr] = values[itr];
			}
		}



		public bool SameInputExtention
		{
			get
			{
				return String.IsNullOrEmpty(OutputExtention);
			}
		}
	}

	
}
