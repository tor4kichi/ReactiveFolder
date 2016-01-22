using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model
{
	[DataContract]
	public class FolderReactionModel : BindableBase
	{

		[DataMember]
		public int ReactionId { get; private set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public bool IsDisable { get; set; }

		// What 対象ファイルやフォルダのフィルター方法
		[DataMember]
		public ReactionFilterBase Filter { get; private set; }



		public FolderReactionModel(int id)
		{
			ReactionId = id;
			Name = "";
			IsDisable = false;
		}

		public bool Validate()
		{

			return true;
		}

		public void Initialize(DirectoryInfo dirInfo)
		{
			Filter.Initialize(dirInfo);
		}

		public IObservable<ReactiveStreamContext> Generate(IObservable<ReactiveStreamContext> stream)
		{
			var first = stream
				// IsDisableがtrueの時はこのリアクションをスキップ
				.SkipWhile(_ => IsDisable);

			var reactionChains = new []{

				Filter
			};

			IObservable<ReactiveStreamContext> chainObserver = first;
			foreach (var chain in reactionChains)
			{
				chainObserver = chain.Chain(chainObserver);
			}


			return chainObserver;
		}
	}
}
