using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Actions
{
	[DataContract]
	public class AppOption
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public Dictionary<string, string> Arguments { get; private set; }

		public AppOption(string name)
		{
			Name = name;
			Arguments = new Dictionary<string, string>();
		}

	}
}
