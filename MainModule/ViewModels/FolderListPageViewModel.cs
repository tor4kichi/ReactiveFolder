using Prism.Mvvm;
using ReactiveFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.ViewModels
{
	public class FolderListPageViewModel : BindableBase
	{
		public FolderReactionMonitorModel MonitorModel { get; private set; }

		public FolderListPageViewModel(FolderReactionMonitorModel monitor)
		{
			MonitorModel = monitor;
		}
	}
}
