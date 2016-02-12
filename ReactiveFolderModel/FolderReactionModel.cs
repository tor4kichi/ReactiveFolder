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

using ReactiveFolder.Models.Filters;
using ReactiveFolder.Models.Timings;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.Destinations;
using Microsoft.Practices.Prism;
using ReactiveFolder.Models.Util;

namespace ReactiveFolder.Models
{
	[DataContract]
	public class FolderReactionModel : BindableBase
	{


		// ***************************

		// TODO: 各Validate系メソッドのリファクタリング



		// ***************************





		[DataMember]
		public Guid Guid { get; private set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public bool IsEnable { get; set; }

		[DataMember]
		public string WorkFolderPath { get; private set; }

		/// <summary>
		/// ワークフォルダが変更されると直ちに
		/// Generate()で作成したストリームを停止させます。
		/// 変更後は再度ストリームを構築してください。
		/// </summary>

		private DirectoryInfo _WorkFolder;
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

					WorkFolderPath = _WorkFolder.FullName;

					ResetWorkingFolder();

					ValidateWorkFolder();
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
				var old = _Filter;
				if (SetProperty(ref _Filter, value))
				{
					if (old != null)
					{
						old.ClearParentReactionModel();
					}

					_Filter.SetParentReactionModel(this);

					ValidateFilter();

					OnPropertyChanged(nameof(InputType));
					OnPropertyChanged(nameof(OutputType));
				}
			}
		}


		[DataMember]
		public FileUpdateReactiveTiming FileUpdateTiming { get; private set; }



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
			private set
			{
				var old = _Destination;
				if (SetProperty(ref _Destination, value))
				{
					if (old != null)
					{
						old.ClearParentReactionModel();
					}

					_Destination?.SetParentReactionModel(this);

					ValidateDestination();
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


		private bool IsNeedValidation;


		

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

				return ValidationResult.IsValid;
			}
		}


		private bool _IsWorkFolderValid;
		public bool IsWorkFolderValid
		{
			get
			{
				return _IsWorkFolderValid;
			}
			set
			{
				SetProperty(ref _IsWorkFolderValid, value);
			}
		}

		private bool _IsActionsValid;
		public bool IsActionsValid
		{
			get
			{
				return _IsActionsValid;
			}
			set
			{
				SetProperty(ref _IsActionsValid, value);
			}
		}

		private bool _IsFilterValid;
		public bool IsFilterValid
		{
			get
			{
				return _IsFilterValid;
			}
			set
			{
				SetProperty(ref _IsFilterValid, value);
			}
		}

		private bool _IsTimingsValid;
		public bool IsTimingsValid
		{
			get
			{
				return _IsTimingsValid;
			}
			set
			{
				SetProperty(ref _IsTimingsValid, value);
			}
		}

		private bool _IsDestinationValid;
		public bool IsDestinationValid
		{
			get
			{
				return _IsDestinationValid;
			}
			set
			{
				SetProperty(ref _IsDestinationValid, value);
			}
		}


		public ValidationResult ValidationResult { get; private set; }
		

		private bool NeedValidation
		{
			get
			{
				return IsNeedValidation ||
					ValidationResult == null;
			}
		}

		/// <summary>
		/// FolderReactionModelを作成します。
		/// idはあなたが管理するオブジェクトから
		/// 一意に判別できる値が割り振られるようにしてください。
		/// </summary>
		/// <param name="id"></param>
		public FolderReactionModel()
		{
			Guid = Guid.NewGuid();
			Name = "";
			IsEnable = true;

			IsNeedValidation = true;

			_Actions = new ObservableCollection<ReactiveActionBase>();
			Actions = new ReadOnlyObservableCollection<ReactiveActionBase>(_Actions);

			FileUpdateTiming = new FileUpdateReactiveTiming();

			var asbDestination = new AbsolutePathReactiveDestination();
			
			Destination = asbDestination;

			CheckInterval = TimeSpan.FromMinutes(1);

			WorkFolder = null;
		}



		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			Actions = new ReadOnlyObservableCollection<ReactiveActionBase>(_Actions);
			
			if (false == String.IsNullOrWhiteSpace(WorkFolderPath))
			{
				WorkFolder = new DirectoryInfo(WorkFolderPath);
			}

			ResetWorkingFolder();
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

			action.SetParentReactionModel(this);

			ValidateActions();

			OnPropertyChanged(nameof(OutputType));
		}


		/// <summary>
		/// アクションを削除します。
		/// <para>このメソッドは最初にExit()を呼び出します。</para>
		/// </summary>
		/// <param name="action"></param>
		public void RemoveAction(ReactiveActionBase action)
		{
			Exit();

			if (_Actions.Remove(action))
			{
				action.ClearParentReactionModel();

				ValidateActions();

				OnPropertyChanged(nameof(OutputType));
			}
		}





		internal void RaisePropertyChangedOnReactiveStream<STREAM_MODEL>(STREAM_MODEL model)
			where STREAM_MODEL: ReactiveStreamBase
		{
			IsNeedValidation = true;

			if (model is ReactiveFilterBase)
			{
				IsFilterValid = false;
				ValidateFilter();
			}
			else if (model is ReactiveActionBase)
			{
				IsActionsValid = false;
				ValidateActions();
			}
			else if (model is ReactiveTimingBase)
			{
				IsTimingsValid = false;
				ValidateTimings();
			}
			else if (model is ReactiveDestinationBase)
			{
				IsDestinationValid = false;
				ValidateDestination();
			}
			else
			{
				throw new Exception("Unknown ReactiveStreamBase derived class. class name: " + nameof(STREAM_MODEL));
			}
		}




		// IFolderItemOutputerを実装する

		public IFolderItemOutputer GetPreviousFolderItemOutputer(ReactiveActionBase source)
		{
			if (false == Actions.Contains(source))
			{
				return Filter;
			}

			if (Actions.Count <= 1)
			{
				return Filter;
			}

			var actionPosition = Actions.IndexOf(source);
			if (actionPosition == 0)
			{
				// Filter
				return Filter;
			}
			else
			{
				return Actions.ElementAt(actionPosition - 1);
			}
		}


		public FolderItemType InputType
		{
			get
			{
				// TODO: Notify
				return Filter.OutputItemType;
			}
		}

		public FolderItemType OutputType
		{
			get
			{
				// TODO: Notify
				if (Actions.Count > 0)
				{
					return Actions.Last().OutputItemType;
				}
				else
				{
					return Filter.OutputItemType;
				}
			}
		}





		public ValidationResult ValidateWorkFolder(ValidationResult outResult = null)
		{
			outResult = outResult ?? new ValidationResult();


			var isWorkFolderValid = WorkFolder?.Exists ?? false;
			if (false == isWorkFolderValid)
			{
				outResult.AddMessage("NOT_EXIST_WORKFODLER");
			}

			IsWorkFolderValid = isWorkFolderValid;

			return outResult;
		}
		

		public ValidationResult ValidateFilter(ValidationResult outResult = null)
		{
			outResult = outResult ?? new ValidationResult();

			if (Filter != null)
			{
				try
				{
					var isValid = Filter.Validate();

					if (false == isValid)
					{
						// Filter validate failed.
						outResult.AddMessage(($"{(nameof(Filter))} has validation error."));
						outResult.AddMessages(Filter.ValidateResult.Messages);
					}
				}
				catch (Exception e)
				{
					outResult.AddMessage(($"{(nameof(Filter))} validation has failed with exception."));
					outResult.AddMessage($"that {(nameof(Filter))} code contains bug or error.");
					outResult.AddMessage($"[Exception Message]:{e.Message}");
				}

				IsFilterValid = Filter.IsValid;
			}
			else
			{
				// Filter not exist.
				outResult.AddMessage(($"{(nameof(Filter))} is must set to Reaction."));

				IsFilterValid = false;
			}


			



			return outResult;
		}




		public ValidationResult ValidateActions(ValidationResult outResult = null)
		{
			outResult = outResult ?? new ValidationResult();

			if (Actions.Count > 0)
			{
				foreach (var action in Actions)
				{
					var isValid = action.Validate();

					if (false == isValid)
					{
						// Action validate failed.
						outResult.AddMessage(($"{(nameof(Actions))} has validation error."));
						outResult.AddMessages(action.ValidateResult.Messages);
					}


				}

				IsActionsValid = Actions.All(x => x.IsValid);
			}
			else
			{
				// Note: アクションが設定されていなければ、ただのコピー動作になる。
				// （出力先はDestinationの設定に依存する）
				// この暗黙のデフォルト動作は想定されたものとして扱うべきか否か…。
				// しかしながら、ユーザーが単純なコピー操作を組み上げたい場合に備えて、
				// TODO: コピー用のアクションを追加するか、デフォルト動作についての説明を用意するか、
				// 判断しないといけない。

				IsActionsValid = true;
			}





			// *************************

			// TODO: Filterで出力したファイルをアクションが処理できるか検証する

			// *************************


			


			return outResult;
		}



		public ValidationResult ValidateTimings(ValidationResult outResult = null)
		{
			outResult = outResult ?? new ValidationResult();


			if (false == FileUpdateTiming.Validate())
			{
				// Timing validate failed.
				outResult.AddMessage(($"Timing has validation error."));
				outResult.AddMessages(FileUpdateTiming.ValidateResult.Messages);
			}
		

			IsTimingsValid = FileUpdateTiming.IsValid;

			return outResult;
		}


		public ValidationResult ValidateDestination(ValidationResult outResult = null)
		{
			outResult = outResult ?? new ValidationResult();

			if (Destination != null)
			{
				var isValid = Destination.Validate();

				if (false == isValid)
				{
					// Destination validate failed.
					outResult.AddMessage(($"{(nameof(Destination))} has validation error."));
					outResult.AddMessages(Destination.ValidateResult.Messages);
				}

				IsDestinationValid = Destination.IsValid;
			}
			else
			{
				// Destination not exist
				outResult.AddMessage(($"{(nameof(Destination))} is must set to Reaction."));

				IsDestinationValid = false;

				// TODO: DestinationがNullである場合って例外処理が必要じゃなイカ？
			}


			


			return outResult;
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


			// WorkFolder/Filter/Actions/Timings/Destinationの検証をまとめて行う

			var Validaters = new Func<ValidationResult, ValidationResult>[] {
				ValidateWorkFolder
				, ValidateFilter
				, ValidateActions
				, ValidateTimings
				, ValidateDestination
			};

			foreach (var validater in Validaters)
			{
				validater(validationResult);
			}


			// モデルを検証実行済みにマーク
			IsNeedValidation = false;


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

			FileUpdateTiming.Initialize(WorkFolder);

			foreach (var action in Actions)
			{
				action.Initialize(WorkFolder);
			}

			Destination?.Initialize(WorkFolder);
		}

		private ReactiveStreamContext CreatePayload()
		{
			if (WorkFolder == null)
			{
				throw new Exception("not exist WorkFolder in FolderReactioinModel.CreatePayload().");
			}

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

			

			

			// Note: なんでTimingsより先にFilterを実行するの？
			// それはね、Timingsにはファイルの更新日時を読んでトリガーするものがあるからさ
			// see@ FileUpdateReactiveTiming.cs


			// Filter
			var filterdTrigger = Filter.Chain(trigger);




			// Timing
			var timingTrigger = FileUpdateTiming.Chain(filterdTrigger);
			

			// Actions
			IObservable<ReactiveStreamContext> actionChainObserver = timingTrigger;
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

			ResetWorkingFolder();

			if (false == IsValid)
			{
				return false;
			}


			if (false == IsEnable)
			{
				return true;
			}



			// タイマーによるトリガーを作成
			var timerTrigger = Observable.Interval(CheckInterval)
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

			ResetWorkingFolder();


			if (false == IsValid)
			{
				return false;
			}



			if (false == this.IsEnable && false == forceEnabling)
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

//			trigger.OnNext(CreatePayload());

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
