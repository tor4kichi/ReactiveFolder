using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model
{
	public interface IStreamContextUpdater
	{
		OutputItemType Update(string sourcePath, DirectoryInfo destFolder);
	}

	public enum OutputItemType
	{
		File,
		Folder,

		Failed,
	}
}
