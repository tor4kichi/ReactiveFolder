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
	// TODO: Generateメソッドがあたかも内部では状態を持たなそうなメソッド名なのに状態を持ってしまっている。


	[Serializable]
	public class FolderReactionGroupModel : BindableBase
	{
		// Note: WorkFolderをNameとして使う
		[DataMember]
		public Guid Guid { get; private set; }

		[DataMember]
		public bool IsDisable { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		private DirectoryInfo _WorkFolder;

		/// <summary>
		/// ワークフォルダが変更されると直ちに
		/// Generate()で作成したストリームを停止させます。
		/// 変更後は再度ストリームを構築してください。
		/// </summary>
		public DirectoryInfo WorkFolder
		{
			get
			{
				return _WorkFolder;
			}
			set
			{
				if (SetProperty(ref _WorkFolder, value))
				{
					Exit();
				}
			}
		}

		[DataMember]
		public ObservableCollection<FolderReactionModel> Reactions { get; private set; }

		public bool IsValidate { get; private set; }

		private Dictionary<FolderReactionModel, BehaviorSubject<ReactionPayload>> TriggerByReaction;

		private IDisposable GroupDisposer;

		public FolderReactionGroupModel(DirectoryInfo dir)
		{
			Guid = Guid.NewGuid();
			WorkFolder = dir;
			Reactions = new ObservableCollection<FolderReactionModel>();
			TriggerByReaction = new Dictionary<FolderReactionModel, BehaviorSubject<ReactionPayload>>();
		}

		public void ValidateCheck()
		{
			IsValidate = false;

			if (false == WorkFolder.Exists)
			{
				Exit();
				return;
			}

			if (Reactions.Count == 0)
			{
				Exit();
				return;
			}

			var invalidReactions = Reactions.Where(x => x.Validate());
			if (invalidReactions.Count() > 0)
			{
				return;
			}


			IsValidate = true;
		}

		

		private void PreGenerate()
		{
			Exit();

			ValidateCheck();

			if (false == IsValidate)
			{
				throw new Exception();
			}

			foreach (var reaction in Reactions)
			{
				reaction.Initialize(WorkFolder);
			}
		}

		private ReactionPayload CreatePayload()
		{
			return new ReactionPayload(WorkFolder, "");
		}

		[OnDeserialized]
		private void SetValuesOnDeserialized(StreamingContext context)
		{
			TriggerByReaction = new Dictionary<FolderReactionModel, BehaviorSubject<ReactionPayload>>();
		}

		public IObservable<ReactionPayload> Generate<T>(IObservable<T> stream)
		{
			TriggerByReaction.Clear();


			PreGenerate();


			var rootStream = stream
				// IsDisableがtrueの時はストリームの流れをスキップさせる
				.SkipWhile(_ => IsDisable)
				// 
				.Do(_ => ValidateCheck())
				// 親ストリームのアイテムを無視して、ReactionPayloadを生成してストリームに流す
				.Select(_ => CreatePayload());
				// 


			List<IConnectableObservable<ReactionPayload>> reactionStreams = new List<IConnectableObservable<ReactionPayload>>();

			foreach(var reaction in Reactions)
			{
				// ストリームの手動実行用トリガーを仕込みつつ、
				var remoteTrigger = new BehaviorSubject<ReactionPayload>(CreatePayload());

				var triggerStream = Observable.Merge(
					reaction.Generate(rootStream),
					remoteTrigger
					)
					.Publish();

				triggerStream
					.Connect();


				reactionStreams.Add(triggerStream);

				TriggerByReaction.Add(reaction, remoteTrigger);
			}

			reactionStreams.ForEach(x => x.Connect());

			var aggregateStream = Observable.Merge(
				reactionStreams
				)
				.Publish();

			GroupDisposer = aggregateStream.Connect();

			return aggregateStream;
		}


		public void Exit()
		{
			GroupDisposer?.Dispose();
			GroupDisposer = null;
		}

		public void Trigger(FolderReactionModel model)
		{
			if (TriggerByReaction.ContainsKey(model))
			{
				var behavior = TriggerByReaction[model];

				behavior.OnNext(CreatePayload());
			}
			else
			{
				throw new Exception(model.Name + "はReactionStreamが生成されていないため実行できません。");
			}
		}

		public void TriggerAll()
		{
			foreach(var reaction in TriggerByReaction.Keys)
			{
				Trigger(reaction);
			}
		}

	}
}
