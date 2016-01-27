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
		public Guid Guid { get; private set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public bool IsDisable { get; set; }

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

					IsReactionParameterChanged = true;
				}
			}
		}

		[DataMember]
		public TimeSpan CheckInterval { get; set; }



		// What 対象ファイルやフォルダのフィルター方法
		[DataMember]
		private ReactiveFilterBase _Filter;
		public ReactiveFilterBase Filter
		{
			get
			{
				return _Filter;
			}
			set
			{
				if (SetProperty(ref _Filter, value))
				{
					IsReactionParameterChanged = true;
				}
			}
		}




		
		
		[DataMember]
		private ObservableCollection<ReactiveTimingBase> _Timings;

		/// <summary>
		/// Actionsの読み取り専用のコレクション。
		/// コレクションの操作はAddAction() または RemoveAction()を利用してください。
		/// </summary>
		public ReadOnlyObservableCollection<ReactiveTimingBase> Timings { get; private set; }



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
				if (SetProperty(ref _Destination, value))
				{
					IsReactionParameterChanged = true;
				}
			}
		}


		public bool IsRunning
		{
			get
			{
				return _Disposer != null;
			}
		}

		private IDisposable _Disposer;


		private BehaviorSubject<ReactiveStreamContext> RemoteTrigger;


		[DataMember]
		private bool IsReactionParameterChanged;


		

		public bool IsValid
		{
			get
			{
				if (NeedValidation)
				{
					ValidationResult = Validate();
					OnPropertyChanged(nameof(ValidationResult));
					OnPropertyChanged(nameof(IsValid));
				}

				return !ValidationResult.HasValidationError;
			}
		}

		public ValidationResult ValidationResult { get; private set; }
		

		private bool NeedValidation
		{
			get
			{
				return IsReactionParameterChanged ||
					ValidationResult == null;
			}
		}

		/// <summary>
		/// FolderReactionModelを作成します。
		/// idはあなたが管理するオブジェクトから
		/// 一意に判別できる値が割り振られるようにしてください。
		/// </summary>
		/// <param name="id"></param>
		public FolderReactionModel(DirectoryInfo targetFolder)
		{
			WorkFolder = targetFolder;
			Guid = Guid.NewGuid();
			Name = "";
			IsDisable = false;

			IsReactionParameterChanged = true;

			_Actions = new ObservableCollection<ReactiveActionBase>();
			Actions = new ReadOnlyObservableCollection<ReactiveActionBase>(_Actions);

			_Timings = new ObservableCollection<ReactiveTimingBase>();
			Timings = new ReadOnlyObservableCollection<ReactiveTimingBase>(_Timings);
		}



		[OnSerialized]
		private void SetValuesOnSerialized(StreamingContext context)
		{
			Actions = new ReadOnlyObservableCollection<ReactiveActionBase>(_Actions);
			Timings = new ReadOnlyObservableCollection<ReactiveTimingBase>(_Timings);
		}

		/// <summary>
		/// タイミングを追加します。
		/// いずれかのタイミングが条件を満たすと後続のAction等が実行されます。
		/// <para>このメソッドは最初にExit()を呼び出します。</para>
		/// </summary>
		/// <param name="timing"></param>
		public void AddTiming(ReactiveTimingBase timing)
		{
			Exit();

			_Timings.Add(timing);

			IsReactionParameterChanged = true;
		}


		/// <summary>
		/// タイミングを削除します。
		/// <para>このメソッドは最初にExit()を呼び出します。</para>
		/// </summary>
		/// <param name="timing"></param>
		public void RemoveTiming(ReactiveTimingBase timing)
		{
			Exit();

			_Timings.Remove(timing);

			IsReactionParameterChanged = true;
		}


		/// <summary>
		/// アクションを追加します。
		/// <para>このメソッドは最初にExit()を呼び出します。</para>
		/// </summary>
		/// <param name="action"></param>
		public void AddAction(ReactiveActionBase action)
		{
			Exit();

			_Actions.Add(action);

			IsReactionParameterChanged = true;
		}


		/// <summary>
		/// アクションを削除します。
		/// <para>このメソッドは最初にExit()を呼び出します。</para>
		/// </summary>
		/// <param name="action"></param>
		public void RemoveAction(ReactiveActionBase action)
		{
			Exit();

			_Actions.Remove(action);

			IsReactionParameterChanged = true;
		}



		/// <summary>
		/// <para>このモデルの妥当性を検証します。</para>
		/// <para>検証対象のプロパティは Filter/Timing/Actions/Destination です。</para>
		/// <para>Filter/Timing/DestinationがNullまたはValidationに失敗した場合、失敗を示します。</para>
		/// <para>Actionsも個々のActionに対して検証を行い、その結果に準じて失敗を示す可能性がありますが、
		/// もし、Actionsの要素が０個の場合は検証は成功を示します。</para>
		/// <para>いずれかのValidationが失敗を示した場合でも、全ての検証を行います。</para>
		/// </summary>
		/// <returns>if no errors, ValidationResult.HasValidationError is false</returns>
		private ValidationResult Validate()
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


			if (Timings.Count > 0)
			{
				foreach (var timing in Timings)
				{
					var result = timing.Validate();

					if (result.HasValidationError)
					{
						// Timing validate failed.
						validationResult.AddMessage(makeValidationErrorMessage($"Timing has validation error."));
						validationResult.AddMessages(result.Messages);
					}
				}
			}
			else
			{
				// タイミングがないと後続のActionを実行できない
				// （ロジック的には、常にActionが実行されてしまうことになる）
				validationResult.AddMessage(makeValidationErrorMessage($"not contain execute timing in Timings."));
			}




			if (Actions.Count > 0)
			{
				foreach (var action in Actions)
				{
					var result = action.Validate();

					if (result.HasValidationError)
					{
						// Action validate failed.
						validationResult.AddMessage(makeValidationErrorMessage($"{(nameof(Actions))} has validation error."));
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
				// TODO: コピー用のアクションを追加するか、デフォルト動作についての説明を用意するか、
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


			IsReactionParameterChanged = false;


			return validationResult;
		}


		/// <summary>
		/// 監視対象となるフォルダを再設定します。
		/// このメソッドは最初にExit()を呼び出します。
		/// </summary>
		/// <param name="folderInfo"></param>
		private void ResetWorkingFolder()
		{
			Filter?.Initialize(WorkFolder);
			foreach (var timing in Timings)
			{
				timing.Initialize(WorkFolder);
			}
			foreach (var action in Actions)
			{
				action.Initialize(WorkFolder);
			}

			Destination?.Initialize(WorkFolder);
		}

		private ReactiveStreamContext CreatePayload()
		{
			return new ReactiveStreamContext(WorkFolder, "");
		}
		
		/// <summary>
		/// 処理ストリームを作成します。
		/// ストリームは引数のtriggerによって実行タイミングが管理されます。
		/// </summary>
		/// <param name="trigger"></param>
		/// <returns></returns>
		private IObservable<ReactiveStreamContext> Generate(IObservable<ReactiveStreamContext> trigger)
		{
			if (false == IsValid)
			{
				throw new Exception();
			}

			

			ResetWorkingFolder();


			// Note: なんでTimingsより先にFilterを実行するの？
			// それはね、Timingsにはファイルの更新日時を読んでトリガーするものがあるからさ
			// see@ FileUpdateReactiveTiming.cs


			// Filter
			var filterdTrigger = Filter.Chain(trigger);




			// Timings
			var timingTriggers = new List<IObservable<ReactiveStreamContext>>();
			foreach (var timing in Timings)
			{
				var timingTrigger = timing.Chain(filterdTrigger);

				timingTriggers.Add(timingTrigger);
			}

			// トリガーストリームを束ねる
			var mergedTimingTrigger = timingTriggers.Merge().Publish();
			mergedTimingTrigger.Connect();


			// Actions
			IObservable<ReactiveStreamContext> actionChainObserver = mergedTimingTrigger;
			foreach (var action in Actions)
			{
				try
				{
					actionChainObserver = action.Chain(actionChainObserver);
				}
				catch (Exception e)
				{
					throw new Exception($"stream generate failed in FolderReactionModel. name:{Name}", e);
				}
			}


			// Destinations
			var finalizedChainObserver = Destination.Chain(actionChainObserver);


			return finalizedChainObserver;
		}



		/// <summary>
		/// <para>監視タスクを開始します。</para>
		/// <para>このメソッドは最初にExit()を呼び出します。</para>
		/// <para>タスクの作成前にValidate()による妥当性チェックが呼び出され
		/// このチェックに失敗した場合、監視タスクは開始されません。</para>
		/// </summary>
		/// <param name="rootTrigger"></param>
		/// <param name="subscriber"></param>
		/// <returns></returns>
		public bool Start(Action<ReactiveStreamContext> subscriber = null)
		{
			Exit();
			
			if (false == IsValid)
			{
				return false;
			}


			if (IsDisable)
			{
				return true;
			}


			// タイマーによるトリガーを作成
			var timerTrigger = Observable.Timer(CheckInterval)
				.Select(_ => CreatePayload());

			var remoteTrigger = new BehaviorSubject<ReactiveStreamContext>(CreatePayload());
			var mergedTrigger =
				Observable.Merge(remoteTrigger, timerTrigger);

			var hotMergedTrigger = mergedTrigger.Publish();


			Generate(hotMergedTrigger)
				.Publish()
				.Connect();


			_Disposer = hotMergedTrigger.Connect();

			RemoteTrigger = remoteTrigger;

			return true;
		}


		public void CheckNow()
		{
			RemoteTrigger?.OnNext(CreatePayload());
		}

		/// <summary>
		/// <para>現在の設定で処理ストリームを作成します。</para>
		/// <para>このメソッドは最初にExit()を呼び出します。</para>
		/// <para>テスト前にValidate()による妥当性チェックが呼び出され
		/// このチェックに失敗した場合、テストは開始されません。</para>
		/// </summary>
		/// <param name="testContext"></param>
		/// <param name="forceEnabling">trueを指定すると、IsDisableの設定を一時的にtrueに設定してテストを開始します。</param>
		public bool Test( bool forceEnabling = false)
		{
			Exit();



			if (false == IsValid)
			{
				return false;
			}



			if (this.IsDisable && false == forceEnabling)
			{
				return true;
			}
			
			// 仮のトリガー
			var trigger = new BehaviorSubject<ReactiveStreamContext>(CreatePayload());

			// ストリームを作る
			var observer = Generate(trigger);

			// ストリームにtestContextのアイテムを流す
			var disposer = observer.Publish()
				.Connect();

			// ストリームを閉じる
			disposer.Dispose();

			return true;
		}



		/// <summary>
		/// 監視タスクを終了します。
		/// </summary>
		public void Exit()
		{
			_Disposer?.Dispose();
			_Disposer = null;
		}
	}



	
}
