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
		private ObservableCollection<AppOptionValueSet> _Options { get; set; }
		public ReadOnlyObservableCollection<AppOptionValueSet> Options { get; private set; }


		public AppOutputFormat(int id)
		{
			_Id = id;
			Name = "";
			OutputExtention = "";

			_Options = new ObservableCollection<AppOptionValueSet>();
			Options = new ReadOnlyObservableCollection<AppOptionValueSet>(_Options);
		}

		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			Options = new ReadOnlyObservableCollection<AppOptionValueSet>(_Options);
		}



		public void AddOrUpdateOption(AppOptionDeclarationBase optionDecl, AppOptionValue[] vals)
		{
			var alreadyPair = _Options.SingleOrDefault(x => x.OptionId == optionDecl.Id);

			if (alreadyPair != null)
			{
				alreadyPair.Values = vals;
			}
			else
			{
				_Options.Add(new AppOptionValueSet()
				{
					OptionId = optionDecl.Id,
					Values = vals
				});
			}

		}


		public void RemoveOption(AppOptionDeclaration optionDecl)
		{
			var val = _Options.SingleOrDefault(x => x.OptionId == optionDecl.Id);

			if (val != null)
			{
				_Options.Remove(val);
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
