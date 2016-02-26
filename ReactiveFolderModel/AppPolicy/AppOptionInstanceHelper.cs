using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.AppPolicy
{
	public static class AppOptionInstanceHelper
	{
		public static AppOptionInstance FindInputInstance(
			this IEnumerable<AppOptionInstance> valueSetList)
		{
			return valueSetList.SingleOrDefault(x => 
				x.OptionDeclaration is AppInputOptionDeclaration
				);
		}

		public static AppOptionInstance FindOutputOptionInstance(
			this IEnumerable<AppOptionInstance> optionInstances)
		{
			return optionInstances.SingleOrDefault(x => 
				x.OptionDeclaration is AppOutputOptionDeclaration
				);
		}
	}
}
