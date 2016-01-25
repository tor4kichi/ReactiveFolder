using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Filters
{
	public static class ReactiveFilterHelper
	{
		public static IEnumerable<string> GetCandidateFilters(FolderReactionModel reaction)
		{
			var buildinItems = new string[]
			{
				"*.*",
				"/*"
			};

			foreach (var buildInText in buildinItems)
			{
				yield return buildInText;
			}

			// 拡張子
			var extentions = reaction.WorkFolder.EnumerateFiles()
				.Select(x => x.Extension)
				.Distinct();

			foreach (var ext in extentions)
			{
				yield return "*" + ext; // ex) *.jpg
			}


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
