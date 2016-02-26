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
		// Note: 現在の状態を利用してFolderReactionを実行します。
		// Folderのモニタリング自体はFolderReactionModelでは行いません。

		// ***************************

		// TODO: 各Validate系メソッドのリファクタリング



		// ***************************





		[DataMember]
		public Guid Guid { get; private set; }

		[DataMember]
		private string _Name;
		public string Name
		{
			get
			{
				return _Name;
			}
			set
			{
				SetProperty(ref _Name, value);
			}
		}

		[DataMember]
		private bool _IsEnable;
		public bool IsEnable
		{
			get
			{
				return _IsEnable;
			}
			set
			{
				SetProperty(ref _IsEnable, value);
			}
		}

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
					WorkFolderPath = _WorkFolder.FullName;

					ResetWorkingFolder();

					ValidateWorkFolder();
				}
			}
		}

		[DataMember]
		private TimeSpan _CheckInterval;
		public TimeSpan CheckInterval
		{
			get
			{
				return _CheckInterval;
			}
			set
			{
				SetProperty(ref _CheckInterval, value);
			}
		}



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
		public AbsolutePathReactiveDestination Destination { get; private set; }
		

		private bool _IsNeedValidation;
		public bool IsNeedValidation
		{
			get
			{
				return _IsNeedValidation;
			}
			private set
			{
				SetProperty(ref _IsNeedValidation, value);
			}
		}


		

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
			FileUpdateTiming.SetParentReactionModel(this);

			Destination = new AbsolutePathReactiveDestination();
			Destination.SetParentReactionModel(this);
			
			CheckInterval = TimeSpan.FromMinutes(1);

			WorkFolder = null;
		}



		[OnDeserialized]
		public void SetValuesOnDeserialized(StreamingContext context)
		{
			if (false == String.IsNullOrWhiteSpace(WorkFolderPath))
			{
				WorkFolder = new DirectoryInfo(WorkFolderPath);
			}

			Actions = new ReadOnlyObservableCollection<ReactiveActionBase>(_Actions);
			

			// ストリーム生成オブジェクトを初期化
			Filter?.SetParentReactionModel(this);
			foreach(var action in Actions)
			{
				action.SetParentReactionModel(this);
			}
			FileUpdateTiming?.SetParentReactionModel(this);
			Destination?.SetParentReactionModel(this);

//			ResetWorkingFolder();
		}


		/// <summary>
		/// アクションを追加します。
		/// <para>このメソッドは最初にExit()を呼び出します。</para>
		/// </summary>
		/// <param name="action"></param>
		public void AddAction(ReactiveActionBase action)
		{
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



		


		public FolderItemType InputType
		{
			get
			{
				return Filter.OutputItemType;
			}
		}

		public FolderItemType OutputType
		{
			get
			{
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

				IsActionsValid = true;
			}

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

		public ReactiveStreamContext CreatePayload()
		{
			if (WorkFolder == null)
			{
				throw new Exception("not exist WorkFolder in FolderReactioinModel.CreatePayload().");
			}

			return new ReactiveStreamContext(WorkFolder, "");
		}
		

		public void Execute(bool forceEnable = false)
		{
			ResetWorkingFolder();

			if (false == IsValid)
			{
				return;
			}


			if (!forceEnable && false == IsEnable)
			{
				return;
			}

			var initialContext = CreatePayload();
			var streams = EnumStreamItems();

			var branchedContexts = Filter.GenerateBranch(initialContext);


			// 実行
			foreach(var context in branchedContexts)
			{
				foreach (var stream in streams)
				{
					if (!context.IsRunnning) break;

					stream.Execute(context);
				}
			}

			// 正常終了した場合の処理
			foreach (var context in branchedContexts.Where(x => x.IsCompleted))
			{
				FileUpdateTiming.OnContextComplete(context);
			}

			// 失敗した処理情報をデバッグ出力
			foreach (var context in branchedContexts.Where(x => x.IsFailed))
			{
				System.Diagnostics.Debug.WriteLine(context.SourcePath);

				if (context.FailedMessage != null)
				{
					System.Diagnostics.Debug.WriteLine(context.FailedMessage);
				}

				if (context.FailedCuaseException != null)
				{
					System.Diagnostics.Debug.WriteLine(context.FailedCuaseException);
				}
			}


			// コンテキストの終了処理
			foreach (var context in branchedContexts)
			{
				context.Dispose();
			}

			// 完了処理
			FileUpdateTiming.OnCompleteReaction();
		}

		private IEnumerable<ReactiveStraightStreamBase> EnumStreamItems()
		{
			yield return FileUpdateTiming;
			foreach(var action in Actions)
			{
				yield return action;
			}

			yield return Destination;
		}



		public static void ResetGuid(FolderReactionModel reaction)
		{
			reaction.Guid = Guid.NewGuid();
		}
	}



	
}
