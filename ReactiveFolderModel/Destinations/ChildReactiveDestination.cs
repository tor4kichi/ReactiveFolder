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
	public class ChildReactiveDestination : ReactiveDestinationBase
	{
		[DataMember]
		public string ChildFolderName { get; set; }

		public override ValidationResult Validate()
		{
			var result = new ValidationResult();

			if (String.IsNullOrWhiteSpace(ChildFolderName))
			{
				result.AddMessage($"{nameof(ChildReactiveDestination)}: Need folder name.");
				return result;
			}


			var invalidChars = Path.GetInvalidPathChars();

			foreach(var invalidChar in invalidChars)
			{
				if (ChildFolderName.Contains(invalidChar))
				{
					result.AddMessage($"{nameof(ChildReactiveDestination)}: contain invalid char '{invalidChar}'");
				}
			}

			return result;
		}

		protected override DirectoryInfo CreateOutputFolder(ReactiveStreamContext context)
		{
			return context.WorkFolder;
		}
	}
}
