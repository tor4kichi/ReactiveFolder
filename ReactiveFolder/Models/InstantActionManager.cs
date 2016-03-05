using ReactiveFolderStyles.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	public class InstantActionManager : IInstantActionManager
	{
		private string _SaveFolder;
		public string SaveFolder
		{
			get
			{
				return _SaveFolder;
			}
			set
			{
				_SaveFolder = value;
				if (false == String.IsNullOrEmpty(_SaveFolder))
				{
					var dir = new DirectoryInfo(_SaveFolder);
					if (false == dir.Exists)
					{
						dir.Create();
					}
				}
			}
		}



		private string _TempSaveFolder;
		public string TempSaveFolder
		{
			get
			{
				return _TempSaveFolder;
			}
			set
			{
				_TempSaveFolder = value;
				if (false == String.IsNullOrEmpty(_TempSaveFolder))
				{
					var dir = new DirectoryInfo(_TempSaveFolder);
					if (false == dir.Exists)
					{
						dir.Create();
					}
				}
			}
		}


	}
}
