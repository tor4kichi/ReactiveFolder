using ReactiveFolder.Model.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model
{
	public interface IStreamContextUpdater : IFolderItemOutputer
	{
		FolderItemType InputItemType { get; }
//		FolderItemType OutputItemType { get; }

		void Update(string sourcePath, DirectoryInfo destFolder);
	}
}
