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

		protected override DirectoryInfo CreateOutputFolder(ReactiveStreamContext context)
		{
			return context.WorkFolder;
		}
	}
}
