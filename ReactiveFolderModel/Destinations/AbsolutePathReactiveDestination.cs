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
	public class AbsolutePathReactiveDestination : ReactiveDestinationBase
	{
		[DataMember]
		public string AbsoluteFolderPath { get; set; }

		public override ValidationResult Validate()
		{
			var result = new ValidationResult();
			if (String.IsNullOrWhiteSpace(AbsoluteFolderPath))
			{
				result.AddMessage($"{(nameof(AbsolutePathReactiveDestination))}: Need path string.");
            }

			if (false == Path.IsPathRooted(AbsoluteFolderPath))
			{
				result.AddMessage($"{(nameof(AbsolutePathReactiveDestination))}: Path is not absolute path.");
			}

			return result;
		}

		protected override DirectoryInfo CreateOutputFolder(ReactiveStreamContext context)
		{
			return context.WorkFolder;
		}
	}
}
