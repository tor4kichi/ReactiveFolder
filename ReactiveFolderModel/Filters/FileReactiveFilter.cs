using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace ReactiveFolder.Model.Filters
{
	[DataContract]
	public class FileReactiveFilter : ReactionFilterBase
	{
		/// <summary>
		/// File Search Pattern
		/// *と?を除いたPath.GetInvalidPathChars()
		/// </summary>

		private static readonly IEnumerable<char> InvalidChars = Path.GetInvalidPathChars().AsEnumerable().Where(x => x == '*' || x == '?');

		/// <summary>
		/// <para>Default is *.*</para>
		/// <para>
		/// *と?のみ特殊文字として扱われます。
		/// * は0文字以上の文字列、
		/// ? は0また1個の文字として扱われます。
		/// ex) *.jpg
		/// </para>
		/// <para>
		/// 正規表現ではありません。
		/// | による複数のパターンの連結は利用できません。
		/// 複数のパターンを指定する場合は新しくFileReactiveFilterを作成してください。
		/// </para>
		/// </summary>
		[DataMember]
		public string FileFilterPattern { get; set; }



		// DirectoryInfo.EnumerateFilesの詳細
		// see@ https://msdn.microsoft.com/ja-jp/library/dd383571(v=vs.110).aspx

		// *と?のみ許可される
		// その他のPath.GetInvalidPathCharsはNG
		public FileReactiveFilter(string pattern = "*.*")
		{
			FileFilterPattern = pattern;
		}

		public override ValidationResult Validate()
		{
			
			var result = new ValidationResult();

			if (String.IsNullOrWhiteSpace(FileFilterPattern))
			{
				result.AddMessage($"{nameof(FileReactiveFilter)}: need file filter pattern. ex) '*.png|*.jpg'");
				return result;
			}

			var invalidChars = InvalidChars;

			foreach (var invalidChar in invalidChars)
			{
				if (FileFilterPattern.Contains(invalidChar))
				{
					result.AddMessage($"{nameof(FileReactiveFilter)}: contain invalid char '{invalidChar}'");
				}
			}

			return result;
		}

		// TODO: ファイル名のフィルタ実装
		public override IEnumerable<FileInfo> FileFilter(DirectoryInfo workDir)
		{
			return workDir.EnumerateFiles(FileFilterPattern, SearchOption.TopDirectoryOnly);
		}

		
	}
}
