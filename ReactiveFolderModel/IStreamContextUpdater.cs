using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model
{
	public interface IStreamContextUpdater
	{
		void Update(string sourcePath, DirectoryInfo destFolder);
	}
}
