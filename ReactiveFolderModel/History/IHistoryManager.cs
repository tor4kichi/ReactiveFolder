using ReactiveFolder.Models;
using System.Collections.Generic;
using System.IO;

namespace ReactiveFolder.Models.History
{
	public interface IHistoryManager
	{
		int AvailableStorageSizeMB { get; }
		string HistorySaveFolderPath { get; }

		List<FileInfo> GetHistoryDataFileList();
		HistoryData LoadHistoryData(FileInfo fileInfo);
		void SaveHistory(HistoryData historyData);
	}
}