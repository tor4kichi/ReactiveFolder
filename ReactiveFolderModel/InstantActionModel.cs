
using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.AppPolicy;
using ReactiveFolder.Models.Destinations;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	[DataContract]
	public class InstantActionSaveModel
	{
		[DataMember]
		public Guid Guid { get; private set; }

		[DataMember]
		public string[] TargetFiles { get; private set; }

		[DataMember]
		public AppLaunchReactiveAction[] Actions { get; private set; }

		[DataMember]
		public string OutputFolderPath { get; private set; }


		

		public static InstantActionSaveModel CreateFromInstantActionModel(InstantActionModel instantAction)
		{
			return new InstantActionSaveModel()
			{
				TargetFiles = instantAction.TargetFiles.Select(x => x.FilePath).ToArray(),
				Actions = instantAction.Actions.ToArray(),
				OutputFolderPath = instantAction.OutputFolderPath
			};
		}

		public static FolderReactionModel CreateReaction(InstantActionSaveModel instantActionSaveModel)
		{
			var reaction = new FolderReactionModel();


			var extentions = instantActionSaveModel.TargetFiles.Select(x => Path.GetExtension(x))
				.Distinct()
				.Select(x => "*" + x);

			foreach (var ext in extentions)
			{
				reaction.Filter.AddIncludeFilter(ext);
			}


			foreach (var action in instantActionSaveModel.Actions)
			{
				reaction.AddAction(action);
			}

			reaction.Destination.AbsoluteFolderPath = instantActionSaveModel.OutputFolderPath;


			return reaction;
		}
	}

	public class InstantActionModel : BindableBase
	{
		public IAppPolicyManager AppPolicyManager { get; private set; }


		public Guid Guid { get; private set; }

		private ObservableCollection<InstantActionTargetFile> _TargetFiles { get; set; }
		public ReadOnlyObservableCollection<InstantActionTargetFile> TargetFiles { get; private set; }


		private ObservableCollection<AppLaunchReactiveAction> _Actions { get; set; }
		public ReadOnlyObservableCollection<AppLaunchReactiveAction> Actions { get; private set; }

		/// <summary>
		/// 処理実行中に出力先フォルダが変更された場合に対応する
		/// </summary>
		private object _OutputFolderPathLock = new object();

		private string _OutputFolderPath;
		public string OutputFolderPath
		{
			get
			{
				return _OutputFolderPath;
			}
			set
			{
				lock(_OutputFolderPathLock)
				{
					if (SetProperty(ref _OutputFolderPath, value))
					{
						// TargetFilesのOutputFolderを書き換える
						foreach (var file in _TargetFiles)
						{
							file.OutputFolderPath = OutputFolderPath;
						}
					}
				}
			}
		}


		


		public InstantActionModel(IAppPolicyManager appPolicyManager)
		{
			AppPolicyManager = appPolicyManager;

			Guid = Guid.NewGuid();
			_TargetFiles = new ObservableCollection<InstantActionTargetFile>();
			TargetFiles = new ReadOnlyObservableCollection<InstantActionTargetFile>(_TargetFiles);
			_Actions = new ObservableCollection<AppLaunchReactiveAction>();
			Actions = new ReadOnlyObservableCollection<AppLaunchReactiveAction>(_Actions);


			OutputFolderPath = "";

		}

		public static InstantActionModel FromSerializedData(InstantActionSaveModel saveModel, IAppPolicyManager appPolicyManager)
		{
			var instantAction = new InstantActionModel(appPolicyManager);

			instantAction.Guid = saveModel.Guid;

			foreach (var targetFile in saveModel.TargetFiles.Select(x => new InstantActionTargetFile(x)))
			{
				instantAction._TargetFiles.Add(targetFile);
			}

			foreach (var action in saveModel.Actions)
			{
				instantAction._Actions.Add(action);
			}

			instantAction.OutputFolderPath = saveModel.OutputFolderPath;

			return instantAction;
		}


		public void AddTargetFile(string path)
		{
			if (_TargetFiles.Any(x => x.FilePath == path))
			{
				return;
			}



			lock (_OutputFolderPathLock)
			{
				var targetFile = new InstantActionTargetFile(path);
				targetFile.OutputFolderPath = this.OutputFolderPath;
				_TargetFiles.Add(targetFile);
			}


		}


		public void RemoveTargetFile(InstantActionTargetFile targetFile)
		{
			_TargetFiles.Remove(targetFile);
		}


		public void AddAction(AppLaunchReactiveAction action)
		{
			_Actions.Add(action);

			ResetTargetFileProcessState();
		}

		public void RemoveAction(AppLaunchReactiveAction action)
		{
			_Actions.Remove(action);

			ResetTargetFileProcessState();
		}


		private void ResetTargetFileProcessState()
		{
			foreach (var targetFile in _TargetFiles)
			{
				targetFile.Ready();
			}
		}


		public ApplicationPolicy[] GetAvailableAppPoliciesOnCurrentFiles()
		{
			var extentions = _TargetFiles
				.Select(x => x.FilePath)
				.Where(x => Path.HasExtension(x))
				.Select(x => Path.GetExtension(x))
				.Distinct();

			var acceptPolicies = AppPolicyManager
				.FindAppPolicyOnAcceptExtentions(extentions);

			return acceptPolicies.ToArray();
		}


		public bool CanExecute
		{
			get
			{
				return true;
			}
		}


		public void Execute(InstantActionTargetFile targetFile)
		{
			lock (_OutputFolderPathLock)
			{
				var reaction = new FolderReactionModel();

				var dir = new DirectoryInfo(Path.GetDirectoryName(targetFile.FilePath));
				using (var context = new ReactiveStreamContext(dir, targetFile.FilePath))
				{
					var dest = new AbsolutePathReactiveDestination();

					if (false == String.IsNullOrEmpty(this.OutputFolderPath) &&
						Directory.Exists(this.OutputFolderPath)
						)
					{
						dest.AbsoluteFolderPath = this.OutputFolderPath;
					}
					else
					{
						targetFile.Failed("File process failed, due to OutputFolder not exist.");
						return;
					}


					var streams = _Actions.Cast<ReactiveStraightStreamBase>().ToList();
					streams.Add(dest);

					foreach (var action in streams)
					{
						if (false == context.IsRunnning)
						{
							break;
						}

						action.Execute(context);
					}



					if (context.IsCompleted)
					{
						targetFile.Complete(context.OutputPath);
					}
					else
					{
						if (context.FailedMessage.Count > 0)
						{
							targetFile.Failed("Failed due to " + context.FailedMessage[0]);
						}
						else
						{
							targetFile.Failed("Failed");
						}
					}

				}
			}

			
		}




		/// <summary>
		/// 一つのオプションだけを扱うアプリ起動アクションを作成する。
		/// </summary>
		/// <param name="appGuid"></param>
		/// <param name="decl"></param>
		/// <returns></returns>
		public static AppLaunchReactiveAction CreateOneOptionAppLaunchAction(Guid appGuid)
		{
			var actionModel = new AppLaunchReactiveAction();
			actionModel.AppGuid = appGuid;

			return actionModel;
		}

		
	}


	public class InstantActionTargetFile : BindableBase
	{
		public string FilePath { get; private set; }


		private string _OutputPath;
		public string OutputPath
		{
			get
			{
				return _OutputPath;
			}
			private set
			{
				SetProperty(ref _OutputPath, value);
			}
		}

		private string _OutputFolderPath;
		public string OutputFolderPath
		{
			get
			{
				return _OutputFolderPath;
			}
			set
			{
				if (SetProperty(ref _OutputFolderPath, value))
				{
					ChangeOutputFolder();
				}
			}
		}

		private FileProcessState _ProcessState;
		public FileProcessState ProcessState
		{
			get
			{
				return _ProcessState;
			}
			private set
			{
				if (SetProperty(ref _ProcessState, value))
				{
					OnPropertyChanged(nameof(IsReady));
					OnPropertyChanged(nameof(IsWaitForProcess));
					OnPropertyChanged(nameof(IsNowProcessing));
					OnPropertyChanged(nameof(IsComplete));
					OnPropertyChanged(nameof(IsFailed));
				}
			}
		}


		private string _ProcessMessage;
		public string ProcessMessage
		{
			get
			{
				return _ProcessMessage;
			}
			set
			{
				SetProperty(ref _ProcessMessage, value);
			}
		}

		public bool IsReady
		{
			get
			{
				return ProcessState == FileProcessState.Ready;
			}
		}

		public bool IsWaitForProcess
		{
			get
			{
				return ProcessState == FileProcessState.WaitForProcess;
			}
		}

		public bool IsNowProcessing
		{
			get
			{
				return ProcessState == FileProcessState.NowProcessing;
			}
		}

		public bool IsComplete
		{
			get
			{
				return ProcessState == FileProcessState.Complete &&
					false == String.IsNullOrWhiteSpace(_OutputPath);
			}
		}

		public bool IsFailed
		{
			get
			{
				return ProcessState == FileProcessState.Failed;
			}
		}

		public void Ready()
		{
			OutputPath = "";
			ProcessState = FileProcessState.Ready;
			ProcessMessage = "";
		}

		public void WaitForProcess()
		{
			ProcessState = FileProcessState.WaitForProcess;
		}

		public void NowProcessing()
		{
			ProcessState = FileProcessState.NowProcessing;
		}

		public void Complete (string path)
		{
			OutputPath = path;
			ProcessState = FileProcessState.Complete;
		}

		
		public void Failed(string message = "")
		{
			ProcessState = FileProcessState.Failed;
			ProcessMessage = message;
		}


		public InstantActionTargetFile(string path)
		{
			FilePath = path;

			Ready();
		}


		private void ChangeOutputFolder()
		{
			if (IsComplete &&
				false == String.IsNullOrEmpty(OutputPath) &&
				File.Exists(OutputPath))
			{
				var sourceFile = new FileInfo(OutputPath);
				var destFilePath = Path.Combine(OutputFolderPath, sourceFile.Name);

				var destFileInfo = new FileInfo(destFilePath);
				// TODO: すでにファイルが存在する場合
				try
				{
					if (destFileInfo.Exists)
					{
						if (destFileInfo.FullName == this.FilePath)
						{
							Failed("Failed due to same file path on both of Input and Output.");
							return;
						}
						else
						{
							destFileInfo.Delete();
						}
					}

					sourceFile.MoveTo(destFileInfo.FullName);
					OutputPath = sourceFile.FullName;
				}
				catch
				{
					Failed("Failed on output file move to OutputFolder");
				}

			}
		}

	}


	public enum FileProcessState
	{
		Ready,
		WaitForProcess,
		NowProcessing,
		Complete,
		Failed,
	}

}
