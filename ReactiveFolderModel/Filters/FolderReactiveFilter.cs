using System.Collections.Generic;
using System.IO;

namespace ReactiveFolder.Model.Filters
{
	public class FolderReactiveFilter : ReactionFilterBase
	{
		protected override IEnumerable<DirectoryInfo> DirectoryFilter(DirectoryInfo workDir)
		{
			return workDir.EnumerateDirectories();
		}
	}
}
