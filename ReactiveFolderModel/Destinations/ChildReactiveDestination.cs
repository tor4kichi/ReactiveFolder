using ReactiveFolder.Model.Util;
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
		private string _ChildFolderName;


		public string ChildFolderName
		{
			get
			{
				return _ChildFolderName;
			}
			set
			{
				SetProperty(ref _ChildFolderName, value);
			}
		}



		public ChildReactiveDestination()
		{
			ChildFolderName = "output";
		}

		public override string GetDistinationFolderPath()
		{
			if (InputFolderInfo == null)
			{
				return null;
			}

			return Path.Combine(InputFolderInfo.FullName, ChildFolderName);
		}


		public override void Initialize(DirectoryInfo workDir)
		{
			InputFolderInfo = workDir;

			base.Initialize(workDir);
		}
	}
}
