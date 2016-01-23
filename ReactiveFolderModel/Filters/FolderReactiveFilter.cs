using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace ReactiveFolder.Model.Filters
{
	[DataContract]
	public class FolderReactiveFilter : ReactionFilterBase
	{
		[DataMember]
		public string FolderFilterPattern { get; set; }


		public FolderReactiveFilter()
		{
			FolderFilterPattern = "*";
		}


		protected override IEnumerable<DirectoryInfo> DirectoryFilter(DirectoryInfo workDir)
		{
			return workDir.EnumerateDirectories(FolderFilterPattern, SearchOption.TopDirectoryOnly);
		}
	}
}
