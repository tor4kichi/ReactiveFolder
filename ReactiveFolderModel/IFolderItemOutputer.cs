using ReactiveFolder.Model.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model
{
	public interface IFolderItemOutputer
	{
		FolderItemType OutputItemType { get; }

		IEnumerable<string> GetFilters();
	}

	
}
