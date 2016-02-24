using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.Actions
{
	abstract public class ReactiveActionBase : ReactiveStraightStreamBase, System.IEquatable<ReactiveActionBase>
	{
		abstract public FolderItemType InputItemType { get; }
		abstract public FolderItemType OutputItemType { get; }

		abstract public bool Equals(ReactiveActionBase other);
	}


}
