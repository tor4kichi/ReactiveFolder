﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolderStyles.Events
{
	public class OpenInstantActionWithFilesEventPayload 
	{
		public string[] FilePaths { get; set; }
	}
}
