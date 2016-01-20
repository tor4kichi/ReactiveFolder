using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model
{
	public class FolderReactionModel
	{
		// What 対象ファイルやフォルダのフィルター方法

		public ReactionTargetFilter Filter { get; private set; }

		// When 実行のタイミング
		// How アクションパイプライン

		

		public FolderReactionModel()
		{
		}

		public IObservable<ReactionPayload> Generate(IObservable<ReactionPayload> stream)
		{
			var first = stream;

			var reactionChains = new []{

				Filter
			};

			foreach(var chain in reactionChains)
			{
				chain.OnReadyToChain();
			}


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
		public virtual void Initialize()
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

		public void OnReadyToChain()
		{
			Context?.Initialize();
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
		private List<PreservedFileInfo> CurrentFiles;
		private List<PreservedFileInfo> PreviewFiles;




		

		public FileUpdateReactionTimingContext()
		{
			PreviewFiles = new List<PreservedFileInfo>();
		}

		public override void Initialize()
		{
			PreviewFiles = CurrentFiles.ToList();
			CurrentFiles.Clear();
		}

		public bool FileIsNeedUpdate(ReactionPayload payload)
		{
			// 
			var needUpdate = PreviewFiles.Any(x =>
			{
				if (payload.Path == x.Path)
				{
					var lastUpdateTime = File.GetLastWriteTime(payload.Path);
					return lastUpdateTime > x.UpdateTime;
				}

				return false;
			});

			PreviewFiles.Add(new PreservedFileInfo(payload.Path));

			return needUpdate;
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
