using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Actions
{
	public abstract class ReactiveActionBase : ReactiveStreamBase
	{
		public override IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev)
		{
			return prev.Do(Reaction);
		}

		public abstract void Reaction(ReactiveStreamContext context);
	}


}
