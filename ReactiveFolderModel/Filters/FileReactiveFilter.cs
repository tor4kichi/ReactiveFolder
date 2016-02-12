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
	public class FileReactiveFilter : ReactiveFilterBase
	{
		/// <summary>
		/// File Search Pattern
		/// *と?を除いたPath.GetInvalidPathChars()
		/// </summary>

		private static readonly IEnumerable<char> InvalidChars = Path.GetInvalidPathChars().AsEnumerable().Where(x => x == '*' || x == '?');

		
		// DirectoryInfo.EnumerateFilesの詳細
		// see@ https://msdn.microsoft.com/ja-jp/library/dd383571(v=vs.110).aspx

		// *と?のみ許可される
		// その他のPath.GetInvalidPathCharsはNG
		public FileReactiveFilter()
		{
		}



		






		protected override ValidationResult InnerValidate()
		{
			var result = new ValidationResult();

			foreach(var fileFilterParttern in IncludeFilter.Concat(ExcludeFilter))
			{
				if (String.IsNullOrWhiteSpace(fileFilterParttern))
				{
					result.AddMessage($"{nameof(FileReactiveFilter)}: need file filter pattern. ex) '*.png|*.jpg'");
					return result;
				}

				foreach (var invalidChar in InvalidChars)
				{
					if (fileFilterParttern.Contains(invalidChar))
					{
						result.AddMessage($"{nameof(FileReactiveFilter)}: contain invalid char '{invalidChar}'");
					}
				}

				try
				{
					IsMatch("aaaa", fileFilterParttern);
				}
				catch
				{
					result.AddMessage($"{nameof(FileReactiveFilter)}: {fileFilterParttern} is not RegularExpression.");
				}
			}			

			return result;
		}

		/// <summary>
		/// FileFilterPatternsを使ってworkDirのトップレベルに存在するファイルをフィルタリングします。
		/// </summary>
		/// <param name="workDir"></param>
		/// <returns></returns>
		public override IEnumerable<FileInfo> FileFilter(DirectoryInfo workDir)
		{
			var files = workDir.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly);

			var filterdFiles = ApplyFilter(files);

			return filterdFiles;
		}

		private IEnumerable<FileInfo> ApplyFilter(IEnumerable<FileInfo> sourceFiles)
		{
			// Note: 包含条件が指定されない場合に全てのファイルを処理してしまうのは不本意の大量データ処理といった事故に繋がる
			// 対象フォルダ内の全件処理はIncludeFilterに"*.*"を明示的に指定された場合に限られる


			// 包含条件
			if (IncludeFilter.Count == 0)
			{
				return Enumerable.Empty<FileInfo>();
			}

			var includeFilteredFiles = new List<FileInfo>();
			foreach (var include in IncludeFilter)
			{
				includeFilteredFiles.AddRange(
					sourceFiles.Where(x => IsMatch(x.Name, include))
					);
			}

			var distinctIncludeFiles = includeFilteredFiles.Distinct();


			// 除外条件
			if (ExcludeFilter.Count == 0)
			{
				return distinctIncludeFiles;
			}

			var excludeFilteredFiles = new List<FileInfo>();
			foreach (var exclude in ExcludeFilter)
			{
				excludeFilteredFiles.AddRange(
					distinctIncludeFiles.Where(x => false == IsMatch(x.Name, exclude))
					);
			}

			var distinctExcludeFiles = excludeFilteredFiles.Distinct();



			return distinctExcludeFiles;
		}

		// 単純なワイルドカード*と0or1文字指定の?を使っている
		// これをRegex.IsMatchのパターンとして渡せる形に変換する
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
				.Replace(".", "\\.")
				.Replace("?", ".?")
				.Replace("*", ".*");
		}

		public bool HasFilter
		{
			get
			{
				return IncludeFilter.Count == 0 && ExcludeFilter.Count == 0;
			}
		}





	

		public IEnumerable<string> FilterWithExtention(IEnumerable<string> extentions)
		{
			if (HasFilter)
			{
				var includeExtentions = IncludeFilter
					.Select(x => x.Substring(x.LastIndexOf(@".")))
					.Distinct();

				var excludeExtentions = ExcludeFilter
					.Select(x => x.Substring(x.LastIndexOf(@".")))
					.Distinct();


				// (extentions * include) - exclude
				return extentions
					.Intersect(includeExtentions)
					.Except(excludeExtentions);
			}
			else
			{
				return Enumerable.Empty<string>();
			}
		}

		// interface IFolderItemOutputer

		public override FolderItemType OutputItemType
		{
			get
			{
				return FolderItemType.File;
			}
		}

		public override IEnumerable<string> GetFilters()
		{
			return this.IncludeFilter
				.Select(x => Path.GetExtension(x))
				.Distinct();
		}
	}
}
