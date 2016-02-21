using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.AppPolicy
{
	public class AppOptionValueSet : IEquatable<AppOptionValueSet>
	{
		public int OptionId { get; set; }
		public AppOptionValue[] Values { get; set; }

		public bool Equals(AppOptionValueSet other)
		{
			return OptionId == other.OptionId;
		}
	}

	
	
}
