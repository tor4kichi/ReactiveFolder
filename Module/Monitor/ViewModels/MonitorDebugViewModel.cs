using Prism.Mvvm;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Monitor.ViewModels
{
	public class MonitorDebugViewModel : BindableBase
	{
		public string MyText { get; private set; }
		public FolderReactionMonitorModel MonitorModel { get; private set; }



		public MonitorDebugViewModel(FolderReactionMonitorModel monitor)
		{
			MonitorModel = monitor;

			MyText = "Hello World!";
		}
	}
}
