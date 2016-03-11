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

		public ValidationResult ValidateResult { get; private set; }


		public AppOptionInstance()
		{
			OptionId = -1;
			Values = null;
			OptionDeclaration = null;
			ValidateResult = new ValidationResult();
		}

		public AppOptionInstance(AppOptionValue[] values, AppOptionDeclarationBase decl)
		{
			OptionId = decl.Id;
			Values = values;
			OptionDeclaration = decl;
			ValidateResult = new ValidationResult();
		}



		public bool Validate()
		{
			ValidateResult.Clear();

			if (OptionDeclaration == null)
			{
				ValidateResult.AddMessage("OptionDeclration is null. AppPolicy or Declaration deleted.");
				return false;
			}

			if (false == OptionDeclaration.CheckValidateOptionValues(this.Values))
			{
				ValidateResult.AddMessage("Invalid option value. may be changed preview Declaration in AppPolicy (or AppPolicy deleted).");
				return false;
			}

			return true;
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
