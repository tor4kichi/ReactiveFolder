using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolderStyles.Events
{
	public class OpenAppPolicyManageEventPayload
	{
	}

	public class OpenAppPolicyWithAppGuidEventPayload
	{
		public Guid AppPolicyGuid { get; set; }
	}
}
