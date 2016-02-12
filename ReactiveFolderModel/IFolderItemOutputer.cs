using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	public interface IFolderItemOutputer
	{
		FolderItemType OutputItemType { get; }

		IEnumerable<string> GetFilters();
	}

	
}
