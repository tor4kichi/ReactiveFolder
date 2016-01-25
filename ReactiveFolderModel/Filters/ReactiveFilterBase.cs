using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model
{
	public class FilterResult
	{
		public IEnumerable<FileInfo> Files { get; set; }
		public IEnumerable<DirectoryInfo> Direcotories { get; set; }
	}

	public abstract class ReactionFilterBase : ReactiveStreamBase
	{
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

		public virtual IEnumerable<FileInfo> FileFilter(DirectoryInfo workDir) { return null; }
		public virtual IEnumerable<DirectoryInfo> DirectoryFilter(DirectoryInfo workDir) { return null; }
	}
}
