using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.AppPolicy
{
	[DataContract]
	public class AppArgument : BindableBase
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
		private string _Description;
		public string Description
		{
			get
			{
				return _Description;
			}
			set
			{
				SetProperty(ref _Description, value);
			}
		}

		[DataMember]
		private string _OptionText;
		public string OptionText
		{
			get
			{
				return _OptionText;
			}
			set
			{
				SetProperty(ref _OptionText, value);
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

		public AppArgument(int id)
		{
			_Id = id;
			Name = "";
			Description = "";
			OptionText = "";
			OutputExtention = "";
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
