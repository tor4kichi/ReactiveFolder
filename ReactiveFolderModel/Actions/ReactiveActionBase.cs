using ReactiveFolder.Model.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Actions
{
	public abstract class ReactiveActionBase : ReactiveStreamBase, IStreamContextUpdater
	{
		abstract public FolderItemType InputItemType { get; }
		abstract public FolderItemType OutputItemType { get; }

		abstract public IEnumerable<string> GetFilters();



		abstract public void Update(string sourcePath, DirectoryInfo destFolder);

		public override IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev)
		{
			return prev.Do((x) =>
			{
				x.Update(this);
			});

		}

	}


}
