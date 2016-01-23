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

using ReactiveFolder.Model.Filters;
using ReactiveFolder.Model.Timings;
using ReactiveFolder.Model.Actions;
using ReactiveFolder.Model.Destinations;
using Microsoft.Practices.Prism;

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
		private ReactionFilterBase _Filter;
		public ReactionFilterBase Filter
		{
			get
			{
				return _Filter;
			}
			set
			{
				SetProperty(ref _Filter, value);
			}
		}

		[DataMember]
		private ReactiveTimingBase _Timing;
		public ReactiveTimingBase Timing
		{
			get
			{
				return _Timing;
			}
			set
			{
				SetProperty(ref _Timing, value);
			}
		}

		[DataMember]
		private ObservableCollection<ReactiveActionBase> _Actions;

		/// <summary>
		/// Actionsの読み取り専用のコレクション。
		/// コレクションの操作はAddAction() または RemoveAction()を利用してください。
		/// </summary>
		public ReadOnlyObservableCollection<ReactiveActionBase> Actions { get; private set; }

		[DataMember]
		private ReactiveDestinationBase _Destination;
		public ReactiveDestinationBase Destination
		{
			get
			{
				return _Destination;
			}
			set
			{
				SetProperty(ref _Destination, value);
			}
		}


		public FolderReactionModel(int id)
		{
			ReactionId = id;
			Name = "";
			IsDisable = false;

			_Actions = new ObservableCollection<ReactiveActionBase>();
			Actions = new ReadOnlyObservableCollection<ReactiveActionBase>(_Actions);
		}


		public void AddAction(ReactiveActionBase action)
		{
			_Actions.Add(action);
		}

		public void RemoveAction(ReactiveActionBase action)
		{
			_Actions.Remove(action);
		}

		/// <summary>
		/// <para>このモデルの妥当性を検証します。</para>
		/// <para>検証対象のプロパティは Filter/Timing/Actions/Destination です。</para>
		/// <para>Filter/Timing/DestinationがNullまたはValidationに失敗した場合、失敗を示します。</para>
		/// <para>Actionsも個々のActionに対して検証を行い、その結果に準じて失敗を示す可能性がありますが、
		/// もし、Actionsの要素が０個の場合は検証は必ず成功を示します。</para>
		/// <para>いずれかのValidation処理に失敗した場合でも、全ての検証を行います。</para>
		/// </summary>
		/// <returns>if no errors, ValidationResult.HasValidationError is false</returns>
		public ValidationResult Validate()
		{
			// 何も問題がなければValidationResultのMessagesは空のまま返されます。
			ValidationResult validationResult = new ValidationResult();


			Func<string, string> makeValidationErrorMessage = (message) =>
			{
				return $"{Name}:{message}";
			};



			if (Filter != null)
			{
				try
				{
					var result = Filter.Validate();

					if (result.HasValidationError)
					{
						// Filter validate failed.
						validationResult.AddMessage(makeValidationErrorMessage($"{(nameof(Filter))} has validation error."));
						validationResult.AddMessages(result.Messages);
					}
				}
				catch(Exception e)
				{
					validationResult.AddMessage(makeValidationErrorMessage($"{(nameof(Filter))} validation has failed with exception."));
					validationResult.AddMessage($"that {(nameof(Filter))} code contains bug or error.");
					validationResult.AddMessage($"[Exception Message]:{e.Message}");
				}
			}
			else
			{
				// Filter not exist.
				validationResult.AddMessage(makeValidationErrorMessage($"{(nameof(Filter))} is must set to Reaction."));
			}



			if (Timing != null)
			{
				var result = Timing.Validate();

				if (result.HasValidationError)
				{
					// Timing validate failed.
					validationResult.AddMessage(makeValidationErrorMessage($"{(nameof(Timing))} has validation error."));
					validationResult.AddMessages(result.Messages);
				}
			}
			else
			{
				// Timing not exist.
				validationResult.AddMessage(makeValidationErrorMessage($"{(nameof(Timing))} is must set to Reaction."));
			}



			if (Actions.Count == 0)
			{
				foreach (var action in Actions)
				{
					var result = action.Validate();

					if (result.HasValidationError)
					{
						// Action validate failed.
						validationResult.AddMessage(makeValidationErrorMessage($"{(nameof(Timing))} has validation error."));
						validationResult.AddMessages(result.Messages);
					}
				}
			}
			else
			{
				// Note: アクションが設定されていなければ、ただのコピー動作になる。
				// （出力先はDestinationの設定に依存する）
				// この暗黙のデフォルト動作は想定されたものとして扱うべきか否か…。
				// しかしながら、ユーザーが単純なコピー操作を組み上げたい場合に備えて、
				// コピー用のアクションを追加するか、デフォルト動作についての説明を用意するか、
				// 判断しないといけない。
			}



			if (Destination != null)
			{
				var result = Destination.Validate();

				if (result.HasValidationError)
				{
					// Destination validate failed.
					validationResult.AddMessage(makeValidationErrorMessage($"{(nameof(Destination))} has validation error."));
					validationResult.AddMessages(result.Messages);
				}
			}
			else
			{
				// Destination not exist
				validationResult.AddMessage(makeValidationErrorMessage($"{(nameof(Destination))} is must set to Reaction."));
			}





			return validationResult;
		}

		public void ResetWorkingFolder(DirectoryInfo folderInfo)
		{
			Filter.Initialize(folderInfo);
			Timing.Initialize(folderInfo);
			foreach(var action in Actions)
			{
				action.Initialize(folderInfo);
			}

			Destination.Initialize(folderInfo);


		}

		public IObservable<ReactiveStreamContext> Generate(IObservable<ReactiveStreamContext> stream)
		{
			var first = stream
				// IsDisableがtrueの時はこのリアクションをスキップ
				.SkipWhile(_ => IsDisable);

			var reactionChains = new List<ReactiveStreamBase>();
			reactionChains.Add(Filter);
			reactionChains.Add(Timing);
			reactionChains.AddRange(Actions);
			reactionChains.Add(Destination);


			
			IObservable<ReactiveStreamContext> chainObserver = first;
			foreach (var reaction in reactionChains)
			{
				try
				{
					chainObserver = reaction.Chain(chainObserver);
				}
				catch(Exception e)
				{
					throw new Exception($"stream generate failed in FolderReactionModel. name:{Name}", e);
				}
			}

			return chainObserver;
		}
	}
}
