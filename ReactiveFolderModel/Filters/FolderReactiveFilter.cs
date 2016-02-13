using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

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


		public override bool IsValidFilterPatternText(string pattern)
		{
			// /folder となる文字列のみを許容する？
			// patternにフォルダで使用できない文字列が含まれていないか

			return Regex.IsMatch(pattern, @"/?[\w\-]+");
		}

		protected override string TransformFilterPattern(string pattern)
		{
			// remove first slash
			return pattern.TrimStart('/');
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
