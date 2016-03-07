using ReactiveFolder.Models.History;
using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	// Note: ファイル処理の実行履歴を管理する

	// TODO: HistorySaveFolderPathが無効の時どうするか。
	// 履歴画面の利用ができないことをUIで通知、設定画面から保存パスを指定を促す

	public class HistoryManager : IHistoryManager
	{
		public const string HISTORY_FILE_EXTENTION = ".rfhistory.json";

		private string _HistorySaveFolderPath;
		public string HistorySaveFolderPath
		{
			get
			{
				return _HistorySaveFolderPath;
			}
			set
			{
				if (_HistorySaveFolderPath != value)
				{
					_HistorySaveFolderPath = value;
					if (_HistorySaveFolderPath != null)
					{
						var dirInfo = new DirectoryInfo(_HistorySaveFolderPath);
						if (false == dirInfo.Exists)
						{
							dirInfo.Create();
						}
					}
				}
			}
		}

		public int AvailableStorageSizeMB { get; private set; }

		public HistoryManager(string historySaveFolderPath, int availableStorageSizeMB)
		{
			HistorySaveFolderPath = historySaveFolderPath;

			AvailableStorageSizeMB = availableStorageSizeMB;
		}

		public HistoryData LoadHistoryData(FileInfo fileInfo)
		{
			return FileSerializeHelper.LoadAsync<HistoryData>(fileInfo);
		}

		

		public void SaveHistory(HistoryData historyData)
		{
			Func<string, string> ToPathSafeString = (string x) =>
			{
				if (String.IsNullOrEmpty(x)) { return ""; }

				foreach(var invalidChar in Path.GetInvalidFileNameChars())
				{
					if (x.Contains(invalidChar))
					{
						x = x.Replace(invalidChar, ' ');
					}
				}

				return x;
			};


			var filename = String.Join("-",
				historyData.Actions.Select(x => ToPathSafeString(x.AppPolicy.AppName) + "(" + String.Join("_", x.AdditionalOptions.Select(y => ToPathSafeString(y.OptionDeclaration.Name))) + ") "
			))
			+ Path.GetRandomFileName();


			var filePath = Path.Combine(HistorySaveFolderPath, Path.ChangeExtension(filename, HISTORY_FILE_EXTENTION));
			FileSerializeHelper.Save(filePath, historyData);
		}

		public List<FileInfo> GetHistoryDataFileList()
		{
			var folderInfo = new DirectoryInfo(HistorySaveFolderPath);

			if (folderInfo.Exists == false)
			{
				folderInfo.Create();
			}

			var files = folderInfo.EnumerateFiles("*" + HISTORY_FILE_EXTENTION, SearchOption.TopDirectoryOnly);

			return files
				.OrderBy(x => x.CreationTime)
				.Reverse()
				.ToList();
		}
	}
}
