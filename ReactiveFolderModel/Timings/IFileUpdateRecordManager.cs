using System.Collections.Generic;
using System.IO;

namespace ReactiveFolder.Models.Timings
{
	public interface IFileUpdateRecordManager
	{
		DirectoryInfo SaveFolder { get; }

		void ClearRecord(FolderReactionModel reaction);
		List<FolderItemUpdateRecord> GetRecord(FolderReactionModel reaction);
		void SaveRecord(FolderReactionModel reaction, List<FolderItemUpdateRecord> records);
		void SetSaveFolder(DirectoryInfo info);
	}
}