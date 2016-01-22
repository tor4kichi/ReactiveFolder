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
		private int ChildReactionIdSeed { get; set; }

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
		private ObservableCollection<FolderReactionModel> _Reactions;

		/// <summary>
		/// 
		/// </summary>
		public ReadOnlyObservableCollection<FolderReactionModel> Reactions { get; private set; }

		public bool IsValidate { get; private set; }

		private Dictionary<FolderReactionModel, BehaviorSubject<ReactiveStreamContext>> TriggerByReaction;

		private IDisposable GroupDisposer;




		public FolderReactionGroupModel(DirectoryInfo dir)
		{
			ChildReactionIdSeed = 1;
			Guid = Guid.NewGuid();
			WorkFolder = dir;
			_Reactions = new ObservableCollection<FolderReactionModel>();
			Reactions = new ReadOnlyObservableCollection<FolderReactionModel>(_Reactions);
			TriggerByReaction = new Dictionary<FolderReactionModel, BehaviorSubject<ReactiveStreamContext>>();
		}




		[OnDeserialized]
		private void SetValuesOnDeserialized(StreamingContext context)
		{
			TriggerByReaction = new Dictionary<FolderReactionModel, BehaviorSubject<ReactiveStreamContext>>();
			Reactions = new ReadOnlyObservableCollection<FolderReactionModel>(_Reactions);
		}



		public FolderReactionModel AddReaction()
		{
			var id = ChildReactionIdSeed;


			FolderReactionModel reaction = new FolderReactionModel(id);

			// TODO: Defaultの設定を追加

			_Reactions.Add(reaction);

			// 
			ChildReactionIdSeed += 1;

			return reaction;
		}

		public void RemoveReaction(FolderReactionModel reaction)
		{
			if (false == _Reactions.Contains(reaction))
			{
				throw new Exception("can not remove FolderReactionModel from FolderReactionGroupModel. argument reaction is not contain FolderReactionGroupModel.");
			}

			RemoveTrigger(reaction);
			_Reactions.Remove(reaction);
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

		private ReactiveStreamContext CreatePayload()
		{
			return new ReactiveStreamContext(WorkFolder, "");
		}

		

		public IObservable<ReactiveStreamContext> Generate<T>(IObservable<T> stream)
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


			List<IConnectableObservable<ReactiveStreamContext>> reactionStreams = new List<IConnectableObservable<ReactiveStreamContext>>();

			foreach(var reaction in Reactions)
			{
				// ストリームの手動実行用トリガーを仕込みつつ、
				var remoteTrigger = new BehaviorSubject<ReactiveStreamContext>(CreatePayload());

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


		private void RemoveTrigger(FolderReactionModel model)
		{
			if (TriggerByReaction.ContainsKey(model))
			{
				TriggerByReaction.Remove(model);
			}
		}
	}
}
