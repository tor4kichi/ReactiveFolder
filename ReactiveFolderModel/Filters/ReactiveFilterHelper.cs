using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Filters
{
	public static class ReactiveFilterHelper
	{


		public static IEnumerable<string> GetFileCandidateFilterPatterns(this FolderReactionModel reaction)
		{
			
			if (reaction.WorkFolder != null)
			{
				yield return "*.*";

				// 拡張子
				var extentions = reaction.WorkFolder.EnumerateFiles()
					.Select(x => x.Extension)
					.Distinct();

				foreach (var ext in extentions)
				{
					yield return "*" + ext; // ex) *.jpg
				}
			}
		}


		public static IEnumerable<string> GetFolderCandidateFilterPatterns(this FolderReactionModel reaction)
		{			
			if (reaction.WorkFolder != null)
			{
				yield return "/*";

				// フォルダ名
				var folders = reaction.WorkFolder.EnumerateDirectories()
					.Select(x => x.Name);

				foreach (var folderName in folders)
				{
					yield return "/" + folderName; // ex) /myfolder
				}
			}
		}
	}
}
