using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	public interface IStreamContextUpdater : IFolderItemOutputer
	{
		FolderItemType InputItemType { get; }
//		FolderItemType OutputItemType { get; }

		void Update(string sourcePath, DirectoryInfo destFolder);
	}
}
