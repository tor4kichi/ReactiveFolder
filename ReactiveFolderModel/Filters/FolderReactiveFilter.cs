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

			return Regex.IsMatch(pattern, @"/?[\w\-\*\?]+");
		}

		protected override string TransformFilterPattern(string pattern)
		{
			// remove first slash
			return pattern.TrimStart('/');
		}


		public override IEnumerable<ReactiveStreamContext> GenerateBranch(ReactiveStreamContext context)
		{
			return DirectoryFilter(context.WorkFolder)
				.Select(x => new ReactiveStreamContext(context.WorkFolder, x.FullName));
		}


		public IEnumerable<DirectoryInfo> DirectoryFilter(DirectoryInfo workDir)
		{
			return workDir.EnumerateDirectories("*.*", SearchOption.TopDirectoryOnly)
				.Where(ApplyFilter);

		}


		private bool ApplyFilter(DirectoryInfo sourceDir)
		{
			// Note: 包含条件が指定されない場合に全てのファイルを処理してしまうのは不本意の大量データ処理といった事故に繋がる
			// 対象フォルダ内の全件処理はIncludeFilterに"*.*"を明示的に指定された場合に限られる
			var sourcePath = sourceDir.FullName;

			// 包含条件に当てはまらない場合
			if (false == IncludeFilter.Any(x => IsMatch(sourcePath, x)))
			{
				return false;
			}

			// 除外条件に当てはまる場合
			if (ExcludeFilter.Any(x => IsMatch(sourcePath, x)))
			{
				return false;
			}

			return true;
		}

		private bool IsMatch(string input, string pattern)
		{
			var regexPatttern = ToRegexParttern(pattern);

			return Regex.IsMatch(input, regexPatttern);
		}

		private string ToRegexParttern(string pattern)
		{
			// .に\\を付け足してエスケープ
			// ?と*をの直前に.を加える

			return pattern
				.Replace("?", ".?")
				.Replace("*", ".*");
		}

		public override IEnumerable<string> GetFilters()
		{
			return IncludeFilter;
		}
	}
}
