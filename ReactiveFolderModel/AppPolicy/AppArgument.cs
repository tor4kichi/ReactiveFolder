using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.AppPolicy
{
	[DataContract]
	public class AppArgument
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string Description { get; set; }

		// TODO: Optionsを削除してKeyValueOptions一本に絞る
		[DataMember]
		public List<string> Options { get; private set; }

		[DataMember]
		public Dictionary<string, string> KeyValueOptions { get; private set; }

		public AppArgument()
		{
			Name = "";
			Description = "";
			Options = new List<string>();
			KeyValueOptions = new Dictionary<string, string>();
		}

	}
}
