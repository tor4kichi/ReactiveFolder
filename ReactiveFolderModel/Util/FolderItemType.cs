using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.Util
{
	public enum FolderItemType
	{
		File,
		Folder,
	}


	public static class FolderItemTypeHelper
	{
		public static FolderItemType FromPath(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
			{
				throw new Exception();
			}
			if (false == Path.IsPathRooted(path))
			{
				throw new Exception();
			}

			if (File.Exists(path))
			{
				return FolderItemType.File;
			}
			else if (Directory.Exists(path))
			{
				return FolderItemType.Folder;
			}
			// pathの特徴から
			else if(false == String.IsNullOrEmpty(Path.GetExtension(path)))
			{
				return FolderItemType.File;
			}

			else if (path.EndsWith("/") || path.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				return FolderItemType.Folder;
			}
			else
			{
				// 判別不能
				throw new Exception("");
			}
		}
	}
}
