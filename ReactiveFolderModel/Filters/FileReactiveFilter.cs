using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace ReactiveFolder.Model.Filters
{
	[DataContract]
	public class FileReactiveFilter : ReactionFilterBase
	{
		[DataMember]
		public string FileFilterPattern { get; set; }

		public FileReactiveFilter()
		{
			FileFilterPattern = "*.*";
		}

		// TODO: ファイル名のフィルタ実装
		protected override IEnumerable<FileInfo> FileFilter(DirectoryInfo workDir)
		{
			return workDir.EnumerateFiles(FileFilterPattern, SearchOption.TopDirectoryOnly);
		}
	}
}
