using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Destinations
{
	public abstract class ReactiveDestinationBase : ReactiveStreamBase
	{
		public override IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev)
		{
			throw new NotImplementedException();
		}


		protected abstract DirectoryInfo CreateOutputFolder(ReactiveStreamContext context);
	}
}
