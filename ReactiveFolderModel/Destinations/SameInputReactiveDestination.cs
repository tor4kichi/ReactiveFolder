using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Destinations
{
	[DataContract]
	public class SameInputReactiveDestination : ReactiveDestinationBase
	{
		public override ValidationResult Validate()
		{
			return ValidationResult.Valid;
		}

		protected override DirectoryInfo CreateOutputFolder(ReactiveStreamContext context)
		{
			return context.WorkFolder;
		}
	}
}
