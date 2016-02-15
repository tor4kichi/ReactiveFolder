using ReactiveFolder.Models.Timings;
using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	public class FileUpdateRecordManager : IFileUpdateRecordManager
	{
		public DirectoryInfo SaveFolder { get; private set; }

		public void SetSaveFolder(DirectoryInfo info)
		{
			if (false == info.Exists)
			{
				info.Create();
			}

			SaveFolder = info;
		}

		public const string UPDATE_RECORD_EXTENTION = ".rfrecord.json";


		private string MakeFilePath(FolderReactionModel reaction)
		{
			return Path.Combine(
				SaveFolder.FullName,
				reaction.Guid.ToString() + UPDATE_RECORD_EXTENTION
				)
				;
		}


		public List<FolderItemUpdateRecord> GetRecord(FolderReactionModel reaction)
		{
			var filename = MakeFilePath(reaction);

			List<FolderItemUpdateRecord> list = null;

			if (File.Exists(filename))
			{
				try
				{
					var record = Util.FileSerializeHelper.LoadAsync<FolderItemUpdateRecord[]>(filename);

					list = record.ToList();
				}
				catch(Exception e)
				{
					File.Delete(filename);
					System.Diagnostics.Debug.WriteLine(e.Message);
				}
			}
			
			return list ?? new List<FolderItemUpdateRecord>();
			
		}

		public void ClearRecord(FolderReactionModel reaction)
		{
			var filename = MakeFilePath(reaction);

			var fileInfo = new FileInfo(filename);
			if (fileInfo.Exists)
			{
				fileInfo.Delete();
			}
		}


		public void SaveRecord(FolderReactionModel reaction, List<FolderItemUpdateRecord> records)
		{
			var filename = MakeFilePath(reaction);

			FileSerializeHelper.Save(filename, records.ToArray());
		}
	}
}
