using Prism.Mvvm;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.ViewModels
{
	public class FolderListPageViewModel : BindableBase
	{

		public string MyText { get; private set; }
		public FolderReactionMonitorModel MonitorModel { get; private set; }



		public FolderListPageViewModel(FolderReactionMonitorModel monitor)
		{
			MonitorModel = monitor;

			MyText = "Hello World!";
		}
	}
}
