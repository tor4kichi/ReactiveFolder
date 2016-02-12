using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace ReactiveFolder.Models.Filters
{
	[DataContract]
	public class FolderReactiveFilter : ReactiveFilterBase
	{
		/// <summary>
		/// DirecotrySearchPattern
		/// *と?を除いたPath.GetInvalidPathChars()
		/// </summary>
		private static readonly IEnumerable<char> InvalidChars = Path.GetInvalidPathChars().AsEnumerable().Where(x => x == '*' || x == '?');


		public override FolderItemType OutputItemType
		{
			get
			{
				return FolderItemType.Folder;
			}
		}

		public FolderReactiveFilter()
		{
		}


		protected override ValidationResult InnerValidate()
		{
			var result = new ValidationResult();

			// TODO: 
			result.AddMessage("not implement");

			return result;
		}

		public override IEnumerable<DirectoryInfo> DirectoryFilter(DirectoryInfo workDir)
		{
			var allDir = workDir.EnumerateDirectories("*.*", SearchOption.TopDirectoryOnly);

			// TODO: 

			return allDir;
		}

		public override IEnumerable<string> GetFilters()
		{
			return IncludeFilter;
		}
	}
}
