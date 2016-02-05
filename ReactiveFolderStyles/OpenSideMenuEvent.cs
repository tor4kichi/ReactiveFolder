using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolderStyles
{
	public enum ReactiveFolderPageType
	{
		ReactiveFolder,
		AppPolicy,
	}

	public class OpenSideMenuEvent : PubSubEvent<ReactiveFolderPageType>
	{
	}
}
