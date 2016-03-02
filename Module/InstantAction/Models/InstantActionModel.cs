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

		public string OutputFolderPath { get; private set; }


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

			TargetFiles.Add(new InstantActionTargetFile(path));
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
			var reaction = new FolderReactionModel();

			var dir = new DirectoryInfo(Path.GetDirectoryName(targetFile.FilePath));
			var context = new ReactiveStreamContext(dir, targetFile.FilePath);

			var dest = new AbsolutePathReactiveDestination();
			dest.AbsoluteFolderPath = ""; // TODO: 出力先

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
				targetFile.OutputPath = context.OutputPath;
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
			set
			{
				SetProperty(ref _OutputPath, value);

				OnPropertyChanged(nameof(IsComplete));
			}
		}


		public bool IsComplete
		{
			get
			{
				return false == String.IsNullOrWhiteSpace(_OutputPath);
			}
		}

		public InstantActionTargetFile(string path)
		{
			FilePath = path;
		}


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
