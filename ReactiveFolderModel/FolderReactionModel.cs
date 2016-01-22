using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model
{
	[DataContract]
	public class FolderReactionModel : BindableBase
	{

		[DataMember]
		public int ReactionId { get; private set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public bool IsDisable { get; set; }

		// What 対象ファイルやフォルダのフィルター方法
		[DataMember]
		public ReactionTargetFilter Filter { get; private set; }



		public FolderReactionModel(int id)
		{
			ReactionId = id;
			Name = "";
			IsDisable = false;
		}

		public bool Validate()
		{

			return true;
		}

		public void Initialize(DirectoryInfo dirInfo)
		{
			Filter.Initialize(dirInfo);
		}

		public IObservable<ReactionPayload> Generate(IObservable<ReactionPayload> stream)
		{
			var first = stream
				// IsDisableがtrueの時はこのリアクションをスキップ
				.SkipWhile(_ => IsDisable);

			var reactionChains = new []{

				Filter
			};

			IObservable<ReactionPayload> chainObserver = first;
			foreach (var chain in reactionChains)
			{
				chainObserver = chain.Chain(chainObserver);
			}


			return chainObserver;
		}
	}


	public class ChainItemContext
	{
		public virtual void Initialize(DirectoryInfo workDir)
		{

		}
	}

	public abstract class ReactionChainItem
	{
		public bool HasParam
		{
			get
			{
				return Context != null;
			}
		}

		public virtual ChainItemContext Context
		{
			get
			{
				return null;
			}
		}

		public void Initialize(DirectoryInfo workDir)
		{
			Context?.Initialize(workDir);
		}

		public abstract IObservable<ReactionPayload> Chain(IObservable<ReactionPayload> prev);
	}


	public class ReactionPayload
	{
		public DirectoryInfo WorkDir { get; set; }
		public string Path { get; set; }


		public ReactionPayload(DirectoryInfo dir, string itempath)
		{
			WorkDir = dir;
			Path = itempath;
		}


		public bool IsFile
		{
			get
			{
				return new FileInfo(Path).Exists;
			}
		}


	}

	// What 




	public class FilterResult
	{
		public IEnumerable<FileInfo> Files { get; set; }
		public IEnumerable<DirectoryInfo> Direcotories { get; set; }
	}
	
	public abstract class ReactionTargetFilter : ReactionChainItem
	{
		public override IObservable<ReactionPayload> Chain(IObservable<ReactionPayload> prev)
		{
			return prev.SelectMany(Filter);
		}

		private IEnumerable<ReactionPayload> Filter(ReactionPayload payload)
		{
			var files = FileFilter(payload.WorkDir);
			foreach (var fileInfo in files)
			{
				yield return new ReactionPayload(payload.WorkDir, fileInfo.FullName);
			}

			var directories = DirectoryFilter(payload.WorkDir);
			foreach (var dirInfo in directories)
			{
				yield return new ReactionPayload(payload.WorkDir, dirInfo.FullName);
			}
		}

		protected virtual IEnumerable<FileInfo> FileFilter(DirectoryInfo workDir) { return null; }
		protected virtual IEnumerable<DirectoryInfo> DirectoryFilter(DirectoryInfo workDir) { return null; }
	}

	public class FileReactionTargetFilter : ReactionTargetFilter
	{
		// TODO: ファイル名のフィルタ実装
		protected override IEnumerable<FileInfo> FileFilter(DirectoryInfo workDir)
		{
			return workDir.EnumerateFiles();
		}
	}

	public class FolderReactionTargetFilter : ReactionTargetFilter
	{
		protected override IEnumerable<DirectoryInfo> DirectoryFilter(DirectoryInfo workDir)
		{
			return workDir.EnumerateDirectories();
		}
	}


	// When

	public abstract class ReactionTimingBase : ReactionChainItem
	{
	}

	struct PreservedFileInfo
	{

		public string Path { get; set; }
		public DateTime UpdateTime { get; set; }

		public PreservedFileInfo(string path)
		{
			Path = path;
			if (File.Exists(path))
			{
				var fileInfo = new FileInfo(Path);
				UpdateTime = fileInfo.LastWriteTime;
			}

			UpdateTime = DateTime.Now;
		}
	}

	class FileUpdateReactionTimingContext : ChainItemContext
	{
		private List<PreservedFileInfo> Files;


		public FileUpdateReactionTimingContext()
		{
			Files = new List<PreservedFileInfo>();
		}

		public override void Initialize(DirectoryInfo workDir)
		{
			Files.AddRange(workDir.GetFiles().Select(x => new PreservedFileInfo(x.FullName)));
			Files.AddRange(workDir.GetDirectories().Select(x => new PreservedFileInfo(x.FullName)));
		}

		public bool FileIsNeedUpdate(ReactionPayload payload)
		{
			var fileAlreadyExist = Files.Any(x => 
			{
				return x.Path == payload.Path;
			});

			if (fileAlreadyExist)
			{
				// すでに同一のPathが登録されていれば
				// 更新時間をチェックして更新が必要か判断する
				var needUpdate = Files.Any(x =>
				{
					if (payload.Path == x.Path)
					{
						var lastUpdateTime = File.GetLastWriteTime(payload.Path);
						return lastUpdateTime > x.UpdateTime;
					}
					return false;
				});

				return needUpdate;
			}
			else
			{
				// まだPathが登録されていない場合は
				// 新しく作成して、更新するよう返す
				Files.Add(new PreservedFileInfo(payload.Path));

				return true;
			}
		}
	}

	public class FileUpdateReactionTiming : ReactionTimingBase
	{

		private FileUpdateReactionTimingContext FileUpdateContext;



		public override ChainItemContext Context
		{
			get
			{
				return FileUpdateContext;
			}
		}


		public FileUpdateReactionTiming()
		{
			FileUpdateContext = new FileUpdateReactionTimingContext();
		}

		public override IObservable<ReactionPayload> Chain(IObservable<ReactionPayload> prev)
		{
			// ファイルやフォルダが更新された作成されていた場合にObservableシーケンスを後続に流す
			return prev.Where(payload => FileUpdateContext.FileIsNeedUpdate(payload));
		}
	}

	public class TimerReactionTiming : ReactionTimingBase
	{
		public DateTimeOffset Time { get; set; }
		public TimeSpan Span { get; set; }

		public TimerReactionTiming(DateTime nextTime, TimeSpan span)
		{
			Time = new DateTimeOffset(nextTime);
		}

		private bool CheckTimingAndUpdateNextTime()
		{
			// 指定時間を過ぎていれば実行
			var timingIsNow = Time < DateTime.Now;

			if (timingIsNow)
			{
				// 正確な次回実行時間の計算
				// 実行間隔 - (現在時刻 - 実行予定時間)
				var overTime = DateTime.Now.Subtract(Time.DateTime);
				var realSpan = Span.Subtract(overTime);
				Time = new DateTimeOffset(Time.DateTime, realSpan);

				while (CheckTimingAndUpdateNextTime()) { }
			}

			return timingIsNow;
		}



		public override IObservable<ReactionPayload> Chain(IObservable<ReactionPayload> prev)
		{
			return prev.Where(_ =>
			{
				return CheckTimingAndUpdateNextTime();
			});
		}
	}


	// How

	public interface IReaction
	{
		IReaction Next { get; set; }

		void Reaction();
	}

	public abstract class ReactionBase : IReaction
	{
		public IReaction Next { get; set; }

		public abstract void Reaction();
	}



	public interface IInputableReaction : IReaction
	{
		ReactionPayload Input { get; set; }
	}

	public interface IOutputableReaction : IReaction
	{
		ReactionPayload Output { get; set; }
	}




}
