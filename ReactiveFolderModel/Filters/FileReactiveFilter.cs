using System.Collections.Generic;
using System.IO;

namespace ReactiveFolder.Model.Filters
{
	public class FileReactiveFilter : ReactionFilterBase
	{
		// TODO: ファイル名のフィルタ実装
		protected override IEnumerable<FileInfo> FileFilter(DirectoryInfo workDir)
		{
			return workDir.EnumerateFiles();
		}
	}
}
