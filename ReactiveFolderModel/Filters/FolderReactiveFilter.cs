using ReactiveFolder.Model.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private ObservableCollection<string> _IncludeFilters { get; set; }


		public ReadOnlyObservableCollection<string> IncludeFilter { get; private set; }


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
		private ObservableCollection<string> _ExcludeFilters { get; set; }
		public ReadOnlyObservableCollection<string> ExcludeFilter { get; private set; }



		public override FolderItemType OutputItemType
		{
			get
			{
				return FolderItemType.Folder;
			}
		}





		public FolderReactiveFilter()
		{
			_IncludeFilters = new ObservableCollection<string>();
			IncludeFilter = new ReadOnlyObservableCollection<string>(_IncludeFilters);
			_ExcludeFilters = new ObservableCollection<string>();
			ExcludeFilter = new ReadOnlyObservableCollection<string>(_ExcludeFilters);
		}


		public void AddIncludeFilter(string pattern)
		{
			if (_IncludeFilters.Contains(pattern))
			{
				return;
			}

			_IncludeFilters.Add(pattern);

			ValidatePropertyChanged();
		}

		public void RemoveInlcudeFilter(string pattern)
		{
			if (_IncludeFilters.Remove(pattern))
			{
				ValidatePropertyChanged();
			}
		}


		public void AddExcludeFilter(string pattern)
		{
			if (_ExcludeFilters.Contains(pattern))
			{
				return;
			}

			_ExcludeFilters.Add(pattern);

			ValidatePropertyChanged();
		}

		public void RemoveExcludeFilter(string pattern)
		{
			if (_ExcludeFilters.Remove(pattern))
			{
				ValidatePropertyChanged();
			}
		}




		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			IncludeFilter = new ReadOnlyObservableCollection<string>(_IncludeFilters);
			ExcludeFilter = new ReadOnlyObservableCollection<string>(_ExcludeFilters);
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
