using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model
{
	public class FolderReactionGroupModel 
	{
		public DirectoryInfo WorkFolder { get; private set; }

		public List<FolderReactionModel> Reactions { get; private set; }

		public FolderReactionGroupModel(DirectoryInfo dir)
		{
			WorkFolder = dir;
			Reactions = new List<FolderReactionModel>();
		}

		public IObservable<ReactionPayload> Generate<T>(IObservable<T> stream)
		{
			var rootStream = stream.Select(_ => new ReactionPayload(WorkFolder, ""));
			return Observable.Merge(
				Reactions.Select(x => x.Generate(rootStream))
				);
		}
	}
}
