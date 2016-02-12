using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.Destinations
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

		public override string GetDistinationFolderPath()
		{
			return AbsoluteFolderPath;
		}
	}
}
