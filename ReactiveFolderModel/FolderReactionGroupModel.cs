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


	[DataContract]
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

		private BehaviorSubject<ReactiveStreamContext> MasterTrigger;


		public TimeSpan CheckInterval { get; set; }


		public bool IsRunning
		{
			get
			{
				return MasterTrigger != null;
			}
		}

		public FolderReactionGroupModel(DirectoryInfo dir, TimeSpan checkInterval)
		{
			WorkFolder = dir;
			CheckInterval = checkInterval;
			ChildReactionIdSeed = 1;
			Guid = Guid.NewGuid();
			_Reactions = new ObservableCollection<FolderReactionModel>();
			Reactions = new ReadOnlyObservableCollection<FolderReactionModel>(_Reactions);
		}




		[OnDeserialized]
		private void SetValuesOnDeserialized(StreamingContext context)
		{
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

			_Reactions.Remove(reaction);

		}

		public ValidationResult Validate()
		{
			var result = new ValidationResult();

			if (false == WorkFolder?.Exists)
			{
				result.AddMessage($"{Name}: Work Folder is not exist.");
			}

			if (Reactions.Count != 0)
			{
				var invalidReactions = Reactions
					.Select(x => x.Validate())
					.Where(x => x.HasValidationError);

				foreach (var react in invalidReactions)
				{
					result.AddMessages(react.Messages);
				}
			}
			else
			{
				result.AddMessage($"{Name}: not have any Reactions.");
			}

			return result;
		}

		

		private ReactiveStreamContext CreatePayload()
		{
			return new ReactiveStreamContext(WorkFolder, "");
		}

		public bool Start(Action<ReactiveStreamContext> subscribe = null)
		{
			Exit();

			var validationResult = this.Validate();
			if (validationResult.HasValidationError)
			{
				return false;
			}

			if (IsDisable)
			{
				return true;
			}

			foreach (var reaction in Reactions)
			{
				reaction.ResetWorkingFolder(WorkFolder);
			}

			var masterTrigger = new BehaviorSubject<ReactiveStreamContext>(CreatePayload());			

			foreach (var reaction in Reactions)
			{
				reaction.Start(masterTrigger, subscribe);
			}

			MasterTrigger = masterTrigger;

			return true;
		}

		public void Exit()
		{
			MasterTrigger?.Dispose();
			MasterTrigger = null;
		}

		

		public void CheckNow()
		{
			MasterTrigger?.OnNext(new ReactiveStreamContext(WorkFolder, ""));
		}		
	}
}
