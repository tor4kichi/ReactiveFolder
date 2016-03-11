using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.Util
{
	public interface IValidatable : INotifyPropertyChanged
	{
		bool IsValid { get; }

		bool Validate();

		ValidationResult ValidateResult { get; }
	}
}
