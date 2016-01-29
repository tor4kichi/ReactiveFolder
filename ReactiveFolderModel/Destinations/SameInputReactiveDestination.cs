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
	public class SameInputReactiveDestination : ReactiveDestinationBase
	{

		private DirectoryInfo InputFolderInfo;

		public override DirectoryInfo GetDestinationFolder()
		{
			return InputFolderInfo;
		}

		public override void Initialize(DirectoryInfo workDir)
		{
			InputFolderInfo = workDir;

			base.Initialize(workDir);
		}

		protected override ValidationResult InnerValidate()
		{
			return ValidationResult.ValidResult;
		}

	}
}
