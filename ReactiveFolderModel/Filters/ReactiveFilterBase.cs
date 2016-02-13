﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Models.Util;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace ReactiveFolder.Models
{
	public class FilterResult
	{
		public IEnumerable<FileInfo> Files { get; set; }
		public IEnumerable<DirectoryInfo> Direcotories { get; set; }
	}

	[DataContract]
	public abstract class ReactiveFilterBase : ReactiveStreamBase, IFolderItemOutputer
	{
		abstract public FolderItemType OutputItemType { get; }

		

		public ReactiveFilterBase()
		{
			_IncludeFilters = new ObservableCollection<string>();
			IncludeFilter = new ReadOnlyObservableCollection<string>(_IncludeFilters);
			_ExcludeFilters = new ObservableCollection<string>();
			ExcludeFilter = new ReadOnlyObservableCollection<string>(_ExcludeFilters);
		}

		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			IncludeFilter = new ReadOnlyObservableCollection<string>(_IncludeFilters);
			ExcludeFilter = new ReadOnlyObservableCollection<string>(_ExcludeFilters);
		}






		
		public virtual IEnumerable<FileInfo> FileFilter(DirectoryInfo workDir) { return null; }
		public virtual IEnumerable<DirectoryInfo> DirectoryFilter(DirectoryInfo workDir) { return null; }

		public abstract IEnumerable<string> GetFilters();



		public override IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev)
		{
			return prev.SelectMany(Filter);
		}


		private IEnumerable<ReactiveStreamContext> Filter(ReactiveStreamContext payload)
		{
			var files = FileFilter(payload.WorkFolder);
			if (files != null)
			{
				foreach (var fileInfo in files)
				{
					yield return new ReactiveStreamContext(payload.WorkFolder, fileInfo.FullName);
				}
			}

			var directories = DirectoryFilter(payload.WorkFolder);
			if (directories != null)
			{
				foreach (var dirInfo in directories)
				{
					yield return new ReactiveStreamContext(payload.WorkFolder, dirInfo.FullName);
				}
			}
		}







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


		abstract public bool IsValidFilterPatternText(string pattern);


		protected override ValidationResult InnerValidate()
		{
			var result = new ValidationResult();

			foreach (var fileFilterParttern in IncludeFilter)
			{
				if (false == IsValidFilterPatternText(fileFilterParttern))
				{
					result.AddMessage("invalid include filter pattern text: " + fileFilterParttern);
				}
			}

			foreach (var fileFilterParttern in ExcludeFilter)
			{
				if (false == IsValidFilterPatternText(fileFilterParttern))
				{
					result.AddMessage("invalid exclude filter pattern text: " + fileFilterParttern);
				}
			}

			return result;
		}


		protected virtual string TransformFilterPattern(string pattern)
		{
			return pattern;
		}

		public void AddIncludeFilter(string pattern)
		{
			var newPattern = TransformFilterPattern(pattern);

			if (false == IsValidFilterPatternText(newPattern))
			{
				return;
			}

			if (_IncludeFilters.Contains(newPattern))
			{
				return;
			}

			_IncludeFilters.Add(newPattern);

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
			var newPattern = TransformFilterPattern(pattern);

			if (false == IsValidFilterPatternText(newPattern))
			{
				return;
			}

			if (_ExcludeFilters.Contains(newPattern))
			{
				return;
			}

			_ExcludeFilters.Add(newPattern);

			ValidatePropertyChanged();
		}

		public void RemoveExcludeFilter(string pattern)
		{
			if (_ExcludeFilters.Remove(pattern))
			{
				ValidatePropertyChanged();
			}
		}

	}
}
