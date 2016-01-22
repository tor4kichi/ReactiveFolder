using System;
using System.IO;

namespace ReactiveFolder.Model
{
	public abstract class ReactiveStreamBase
	{
		public virtual void Initialize(DirectoryInfo workDir)
		{
		}

		public abstract IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev);
	}
	
}
