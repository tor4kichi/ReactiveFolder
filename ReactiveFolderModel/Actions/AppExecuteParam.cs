using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Actions
{
	[DataContract]
	public class AppArgument
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public List<string> Options { get; private set; }

		[DataMember]
		public Dictionary<string, string> KeyValueOptions { get; private set; }

		public AppArgument(string name)
		{
			Name = name;
			Options = new List<string>();
			KeyValueOptions = new Dictionary<string, string>();
		}

	}
}
