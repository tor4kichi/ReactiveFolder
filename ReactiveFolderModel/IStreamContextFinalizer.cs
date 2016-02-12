﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	public interface IStreamContextFinalizer
	{
		string OutputName { get; }
		DirectoryInfo GetDestinationFolder();
	}
}
