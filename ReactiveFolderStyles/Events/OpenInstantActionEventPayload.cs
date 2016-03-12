using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolderStyles.Events
{
	public class OpenInstantActionEventPayload
	{
	}

	public class OpenInstantActionWithFilePathEventPayload
	{
		public string FilePath { get; set; }
	}
}
