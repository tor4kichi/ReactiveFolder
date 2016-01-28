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
		private string _AbsoluteFolderPath;
		public string AbsoluteFolderPath
		{
			get
			{
				return _AbsoluteFolderPath;
			}
			set
			{
				SetProperty(ref _AbsoluteFolderPath, value);
			}
		}


		public AbsolutePathReactiveDestination()
		{
			AbsoluteFolderPath = "";
		}

		public override DirectoryInfo GetDestinationFolder()
		{
			return new DirectoryInfo(AbsoluteFolderPath);
		}

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
	}
}
