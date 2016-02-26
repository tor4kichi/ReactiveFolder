using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Practices.Prism.Mvvm;

namespace ReactiveFolder.Models.AppPolicy
{
	[DataContract]
	public class AppOptionInstance : BindableBase, IEquatable<AppOptionInstance>, IValidatable
	{

		[DataMember]
		public int OptionId { get; private set; }


		[DataMember]
		public AppOptionValue[] Values { get; set; }

		public AppOptionDeclarationBase OptionDeclaration { get; private set; }

		public bool IsValid
		{
			get
			{
				throw new NotImplementedException();
			}
		}


		

		public AppOptionInstance(int optionId, AppOptionValue[] values, AppOptionDeclarationBase decl)
		{
			OptionId = optionId;
			Values = values;
			OptionDeclaration = decl;
		}



		public bool Validate()
		{
			throw new NotImplementedException();
		}

		public void ResetDeclaration(ApplicationPolicy appPolicy)
		{
			this.OptionDeclaration = appPolicy.FindOptionDeclaration(OptionId);
		}



		public bool Equals(AppOptionInstance other)
		{
			return OptionId == other.OptionId;
		}

	}

	
	
}
