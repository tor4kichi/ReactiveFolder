using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Destinations
{
	public abstract class ReactiveDestinationBase : ReactiveStreamBase, IStreamContextFinalizer
	{
		public abstract DirectoryInfo GetDestinationFolder();

		public override void Initialize(DirectoryInfo workDir)
		{
			var destFolder = GetDestinationFolder();
			if (false == destFolder.Exists)
			{
				destFolder.Create();
			}

			base.Initialize(workDir);
		}
		public override IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev)
		{
			return prev.Do(context => context.Finalize(this));
		}
	}
}
