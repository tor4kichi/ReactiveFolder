using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace ReactiveFolder.Model.Filters
{
	[DataContract]
	public class FileReactiveFilter : ReactiveFilterBase
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
		public ObservableCollection<string> FileFilterPatterns { get; set; }



		// DirectoryInfo.EnumerateFilesの詳細
		// see@ https://msdn.microsoft.com/ja-jp/library/dd383571(v=vs.110).aspx

		// *と?のみ許可される
		// その他のPath.GetInvalidPathCharsはNG
		public FileReactiveFilter(params string[] defaultFilterpatters)
		{
			if (defaultFilterpatters == null)
			{
				defaultFilterpatters = new string[] { };
			}
			FileFilterPatterns = new ObservableCollection<string>(defaultFilterpatters);
		}

		public override ValidationResult Validate()
		{
			
			var result = new ValidationResult();

			foreach(var fileFilterParttern in FileFilterPatterns)
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
			foreach(var pattern in FileFilterPatterns)
			{
				var files = workDir.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly);
				foreach (var file in files)
				{
					yield return file;
				}
			}
		}

		
	}
}
