using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Util
{
	public interface IValidatable : INotifyPropertyChanged
	{
		bool IsValid { get; }

		bool Validate();
	}
}
