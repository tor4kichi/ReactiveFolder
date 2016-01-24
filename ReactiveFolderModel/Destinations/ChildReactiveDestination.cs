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
		private DirectoryInfo InputFolderInfo;


		[DataMember]
		public string ChildFolderName { get; set; }

		public override DirectoryInfo GetDestinationFolder()
		{
			return new DirectoryInfo(
				Path.Combine(InputFolderInfo.FullName, ChildFolderName)
				);
		}

		public override void Initialize(DirectoryInfo workDir)
		{
			InputFolderInfo = workDir;

			base.Initialize(workDir);
		}

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

			var destFolder = GetDestinationFolder();

			if (false == destFolder.Exists)
			{
				throw new Exception("not call initialize?");
			}

			return result;
		}
	}
}
