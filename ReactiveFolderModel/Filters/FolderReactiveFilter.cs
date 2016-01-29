using ReactiveFolder.Model.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace ReactiveFolder.Model.Filters
{
	[DataContract]
	public class FolderReactiveFilter : ReactiveFilterBase
	{
		/// <summary>
		/// DirecotrySearchPattern
		/// *と?を除いたPath.GetInvalidPathChars()
		/// </summary>
		private static readonly IEnumerable<char> InvalidChars = Path.GetInvalidPathChars().AsEnumerable().Where(x => x == '*' || x == '?');




		[DataMember]
		private string _FolderFilterPattern;

		/// <summary>
		/// <para>Default is *</para>
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
		public string FolderFilterPattern
		{
			get
			{
				return _FolderFilterPattern;
			}
			set
			{
				SetProperty(ref _FolderFilterPattern, value);
			}

		}




		public FolderReactiveFilter(string folderFilterPattern = "*")
		{
			FolderFilterPattern = folderFilterPattern;
		}




		protected override ValidationResult InnerValidate()
		{
			var result = new ValidationResult();

			if (String.IsNullOrWhiteSpace(FolderFilterPattern))
			{
				result.AddMessage($"{nameof(FileReactiveFilter)}: need file filter pattern. ex) '*.png|*.jpg'");
				return result;
			}

			var invalidChars = InvalidChars;

			foreach (var invalidChar in InvalidChars)
			{
				if (FolderFilterPattern.Contains(invalidChar))
				{
					result.AddMessage($"{nameof(FileReactiveFilter)}: contain invalid char '{invalidChar}'");
				}
			}

			return result;
		}

		public override IEnumerable<DirectoryInfo> DirectoryFilter(DirectoryInfo workDir)
		{
			return workDir.EnumerateDirectories(FolderFilterPattern, SearchOption.TopDirectoryOnly);
		}
	}
}
