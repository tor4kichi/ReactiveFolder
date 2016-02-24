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
				x.OptionDeclaration is AppInputPathOptionDeclaration
				);
		}

		public static IEnumerable<AppOptionInstance> FindOutputOptionInstances(
			this IEnumerable<AppOptionInstance> optionInstances)
		{
			return optionInstances.Where(x => 
				x.OptionDeclaration is AppOutputPathOptionDeclaration
				);
		}
	}
}
