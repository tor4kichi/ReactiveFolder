using Prism.Mvvm;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.AppPolicy;
using ReactiveFolder.Models.Destinations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Modules.InstantAction.Models
{
	[DataContract]
	public class InstantActionSaveModel
	{
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
	}

	public class InstantActionModel : BindableBase
	{
		public IAppPolicyManager AppPolicyManager { get; set; }


		public ObservableCollection<InstantActionTargetFile> TargetFiles { get; private set; }

		public ObservableCollection<AppLaunchReactiveAction> Actions { get; private set; }

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
						foreach (var file in TargetFiles)
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

			TargetFiles = new ObservableCollection<InstantActionTargetFile>();
			Actions = new ObservableCollection<AppLaunchReactiveAction>();
			OutputFolderPath = "";
		}


		public static InstantActionModel FromInstantAction(InstantActionSaveModel saveModel, IAppPolicyManager appPolicyManager)
		{
			var instantAction = new InstantActionModel(appPolicyManager);

			instantAction.TargetFiles.AddRange(saveModel.TargetFiles.Select(x => new InstantActionTargetFile(x)));
			instantAction.Actions.AddRange(saveModel.Actions);
			instantAction.OutputFolderPath = saveModel.OutputFolderPath;

			return instantAction;
		}


		public void AddTargetFile(string path)
		{
			if (TargetFiles.Any(x => x.FilePath == path))
			{
				return;
			}



			lock (_OutputFolderPathLock)
			{
				var targetFile = new InstantActionTargetFile(path);
				targetFile.OutputFolderPath = this.OutputFolderPath;
				TargetFiles.Add(targetFile);
			}


		}


		public InstantAppOption[] GenerateAppOptions()
		{
			var extentions = TargetFiles
				.Select(x => x.FilePath)
				.Where(x => Path.HasExtension(x))
				.Select(x => Path.GetExtension(x))
				.Distinct();

			var acceptPolicies = AppPolicyManager
				.FindAppPolicyOnAcceptExtentions(extentions);

			return acceptPolicies
				.SelectMany(FromAppPolicy)
				.ToArray();
		}



		public IEnumerable<InstantAppOption> FromAppPolicy(ApplicationPolicy appPolicy)
		{
			foreach (var optDecl in appPolicy.OptionDeclarations)
			{
				yield return new InstantAppOption(appPolicy, optDecl);
			}

			foreach (var outputOptDecl in appPolicy.OutputOptionDeclarations)
			{
				yield return new InstantAppOption(appPolicy, outputOptDecl);
			}
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
						targetFile.Failed();
						return;
					}


					var streams = Actions.Cast<ReactiveStraightStreamBase>().ToList();
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
						targetFile.Failed();
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
		public static AppLaunchReactiveAction CreateOneOptionAppLaunchAction(Guid appGuid, AppOptionDeclarationBase decl)
		{
			var actionModel = new AppLaunchReactiveAction();
			actionModel.AppGuid = appGuid;

			var optionInstance = decl.CreateInstance();
			actionModel.AddAppOptionInstance(optionInstance);

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

		
		public void Failed()
		{
			ProcessState = FileProcessState.Failed;
		}


		public InstantActionTargetFile(string path)
		{
			FilePath = path;

			ProcessState = FileProcessState.Ready;
		}


		private void ChangeOutputFolder()
		{
			if (IsComplete &&
				false == String.IsNullOrEmpty(OutputPath) &&
				File.Exists(OutputPath))
			{
				var sourceFile = new FileInfo(OutputPath);
				var destFilePath = Path.Combine(OutputFolderPath, sourceFile.Name);


				// TODO: すでにファイルが存在する場合
				try
				{
					sourceFile.MoveTo(destFilePath);
					OutputPath = sourceFile.FullName;
				}
				catch
				{
					Failed();
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

	public class InstantAppOption 
	{
		public ApplicationPolicy AppPolicy { get; private set; }
		public AppOptionDeclarationBase Declaration { get; private set; }

		public InstantAppOption(ApplicationPolicy appPolicy, AppOptionDeclarationBase decl)
		{
			AppPolicy = appPolicy;
			Declaration = decl;
		}
	}
}
