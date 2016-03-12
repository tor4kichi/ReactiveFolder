using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolderStyles.Events
{
	public class OpenHisotryPageEventPayload
	{

	}

	public class OpenHisotryWithAppPolicyPageEventPayload
	{
		public Guid AppPolicyGuid { get; set; }
	}

	public class OpenHisotryWithReactionPageEventPayload
	{
		public Guid ReactionGuid { get; set; }
	}
}
