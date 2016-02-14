using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	public class FolderReactionMonitorModel : BindableBase, IFolderReactionMonitorModel
	{
		private Dictionary<Guid, ReactionMonitor> _RunningReactions;

	
		private DirectoryInfo _ReactionSaveFolder;
		public DirectoryInfo ReactionSaveFolder
		{
			get
			{
				return _ReactionSaveFolder;
			}
			set
			{
				if (false == value.Exists)
				{
					value.Create();
				}

				_ReactionSaveFolder = value;
			}
		}



		public FolderModel RootFolder { get; private set; }

		private TimeSpan _DefaultInterval;
		public TimeSpan DefaultInterval
		{
			get
			{
				return _DefaultInterval;
			}
			set
			{
				SetProperty(ref _DefaultInterval, value);
			}
		}		

		public FolderReactionMonitorModel(DirectoryInfo saveFolder)
		{
			_RunningReactions = new Dictionary<Guid, ReactionMonitor>();

			ReactionSaveFolder = saveFolder;

			DefaultInterval = TimeSpan.FromMinutes(15);

			InitializeReactions();
		}



		private void InitializeReactions()
		{
			RootFolder = FolderModel.LoadFolder(ReactionSaveFolder);
		}


		public void Dispose()
		{
			Exit();
		}

		



		public FolderReactionModel FindReaction(Guid guid)
		{
			return RootFolder.FindReaction(guid);
		}

		public FolderModel FindReactionParentFolder(Guid guid)
		{
			return RootFolder.FindReactionParent(guid);
		}

		public FolderModel FindReactionParentFolder(FolderReactionModel model)
		{
			return RootFolder.FindReactionParent(model);
		}




		public FolderModel FindFolder(string path)
		{
			return RootFolder.FindFolder(path);
		}


		



		#region Monitor Manage

		public void Start()
		{
			StartMonitoring();
		}


		public void Exit()
		{
			StopMonitoring();
		}




		public void PauseMonitoring(FolderReactionModel reaction)
		{
			if (_RunningReactions.ContainsKey(reaction.Guid))
			{
				var monitor = _RunningReactions[reaction.Guid];

				monitor.IsPause = true;
			}
		}

		public void ResumeMonitoring(FolderReactionModel reaction)
		{
			if (_RunningReactions.ContainsKey(reaction.Guid))
			{
				var monitor = _RunningReactions[reaction.Guid];

				monitor.IsPause = false;
			}
		}


		


		private void StartMonitoring()
		{
			RecursiveFolder(RootFolder, CheckRunOrStopReactionMonitoring);
		}

		private void StopMonitoring()
		{
			RecursiveFolder(RootFolder, StopReactionMonitoring);
		}

		private void RecursiveFolder(FolderModel folder, Action<FolderReactionModel> act)
		{
			foreach (var reaction in folder.Models)
			{
				act(reaction);
			}

			// check child folder
			foreach (var childFolder in folder.Children)
			{
				RecursiveFolder(childFolder, act);
			}
		}

		private void CheckRunOrStopReactionMonitoring(FolderReactionModel reaction)
		{

			// Reaction start?
			if (reaction.IsEnable && reaction.IsValid)
			{
				// start
				if (false == _RunningReactions.ContainsKey(reaction.Guid))
				{
					var monitor = StartReactionMonitor(reaction);

					_RunningReactions.Add(reaction.Guid, monitor);
				}
			}
			else
			{
				// stop
				StopReactionMonitoring(reaction);
			}
		}

		private ReactionMonitor StartReactionMonitor(FolderReactionModel reaction)
		{
			return new ReactionMonitor(reaction);
		}


		private void StopReactionMonitoring(FolderReactionModel reaction)
		{
			if (_RunningReactions.ContainsKey(reaction.Guid))
			{
				var monitor = _RunningReactions[reaction.Guid];

				monitor.Dispose();

				_RunningReactions.Remove(reaction.Guid);
			}
		}





		#endregion


	}



	public class ReactionMonitor : IDisposable
	{
		public FolderReactionModel Reaction { get; private set; }

		public bool NowProcessing { get; private set; }
		public bool IsPause { get; set; }
		public bool IsTerminated { get; set; }

		private IDisposable _Disposer;

		public Exception TerminateCauseException { get; private set; }

		public ReactionMonitor(FolderReactionModel reaction)
		{
			Reaction = reaction;
			IsPause = false;
			NowProcessing = false;
			IsTerminated = false;

			Start();
		}

		public void Dispose()
		{
			_Disposer?.Dispose();
			_Disposer = null;
		}

		public void Start()
		{
			Dispose();

			_Disposer = Observable.Interval(Reaction.CheckInterval)
				.Where(_ => CanExecute)
				.Do(cnt => ExecuteReaction(cnt))
				.Subscribe();
		}


		public bool CanExecute
		{
			get
			{
				return
					false == this.IsPause &&
					false == this.NowProcessing &&
					false == this.IsTerminated;
			}
		}

		public void ExecuteReaction(long count = -1)
		{
			PreTrigger(count);
			TriggerReactioin();
			PostTrigger();
		}

		private void PreTrigger(long count = -1)
		{
			NowProcessing = true;

			if (count == -1)
			{
				System.Diagnostics.Debug.WriteLine($"{Reaction.Name}の処理を開始しました。");
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"{Reaction.Name}の{count+1}回目の処理を開始しました。");
			}
        }

		private void TriggerReactioin()
		{
			// TODO: lock
			try
			{
				Reaction.Execute();
			}
			catch(Exception e)
			{
				IsTerminated = true;
				TerminateCauseException = e;
				Dispose();
			}
		}

		private void PostTrigger()
		{
			NowProcessing = false;

			System.Diagnostics.Debug.WriteLine($"{Reaction.Name}の処理が終了しました。");
		}

		
	}
}
