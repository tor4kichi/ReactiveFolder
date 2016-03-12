using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Actions;
using ReactiveFolder.Models.History;
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

		public IHistoryManager HistoryManager { get; private set; }

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

		public FolderReactionMonitorModel(DirectoryInfo saveFolder, IHistoryManager historyManager)
		{
			HistoryManager = historyManager;
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
			StopAllReactionMonitoring();
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


		public void StartMonitoring(FolderReactionModel reaction)
		{
			StopMonitoring(reaction);

			if (reaction.IsEnable && reaction.IsValid)
			{
				_RunningReactions.Add(reaction.Guid, new ReactionMonitor(reaction, this, HistoryManager));
			}
		}

		public void StopMonitoring(FolderReactionModel reaction)
		{
			if (_RunningReactions.ContainsKey(reaction.Guid))
			{
				var monitor = _RunningReactions[reaction.Guid];

				monitor.Dispose();

				_RunningReactions.Remove(reaction.Guid);
			}
		}


		


		public void StartAllReactionMonitoring()
		{
			RecursiveFolder(RootFolder, StartMonitoring);
		}

		public void StopAllReactionMonitoring()
		{
			RecursiveFolder(RootFolder, StopMonitoring);
		}

		private void RecursiveFolder(FolderModel folder, Action<FolderReactionModel> act)
		{
			foreach (var reaction in folder.Reactions)
			{
				act(reaction);
			}

			// check child folder
			foreach (var childFolder in folder.Children)
			{
				RecursiveFolder(childFolder, act);
			}
		}

		


		#endregion


	}



	public class ReactionMonitor : IDisposable
	{
		public FolderReactionModel Reaction { get; private set; }
		public IFolderReactionMonitorModel Monitor { get; private set; }
		public IHistoryManager HistoryManager { get; private set; }

		public bool NowProcessing { get; private set; }
		public bool IsTerminated { get; set; }

		private IDisposable _Disposer;

		public Exception TerminateCauseException { get; private set; }

		public ReactionMonitor(FolderReactionModel reaction, IFolderReactionMonitorModel monitor, IHistoryManager history)
		{
			Reaction = reaction;
			Monitor = monitor;
			HistoryManager = history;

			NowProcessing = false;
			IsTerminated = false;

			Start();
		}

		public void Dispose()
		{
			if (_Disposer != null)
			{
				_Disposer.Dispose();
				_Disposer = null;

				System.Diagnostics.Debug.WriteLine($"exit monitoring : {Reaction.Name}");
			}
			
		}

		public void Start()
		{
			Dispose();

			_Disposer = Observable.Interval(Reaction.CheckInterval)
				.Where(_ => CanExecute)
				.Do(cnt => ExecuteReaction(cnt))
				.Subscribe();

			System.Diagnostics.Debug.WriteLine($"start monitoring : {Reaction.Name}");

			if (CanExecute)
			{
				ExecuteReaction();
			}
		}


		public bool CanExecute
		{
			get
			{
				return
					false == this.NowProcessing &&
					false == this.IsTerminated;
			}
		}

		public void ExecuteReaction(long count = -1)
		{
			PreTrigger(count);
			TriggerReaction();
			PostTrigger();
		}

		private void PreTrigger(long count = -1)
		{
			NowProcessing = true;

			if (count == -1)
			{
				System.Diagnostics.Debug.WriteLine($"check reaction on remote : {Reaction.Name}");
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"check reaction on timer [{count + 1}] : {Reaction.Name}");
			}
		}

		private void TriggerReaction()
		{
			HistoryDataByFile[] results = null;


			try
			{
				results = Reaction.Execute();
			}
			catch(Exception e)
			{
				IsTerminated = true;
				TerminateCauseException = e;
				Dispose();
			}


			if (results != null)
			{
				var historyData = new HistoryData()
				{
					Actions = Reaction.Actions.Select(x => x as AppLaunchReactiveAction).ToArray(),
					ActionSourceFilePath = Monitor.RootFolder.MakeReactionSaveFilePath(Reaction),
					FileHistories = results
				};

				HistoryManager.SaveHistory(historyData);
			}
		}

		private void PostTrigger()
		{
			NowProcessing = false;

			if (IsTerminated)
			{
				System.Diagnostics.Debug.WriteLine($"terminated reaction : {Reaction.Name}");
				System.Diagnostics.Debug.WriteLine(TerminateCauseException.Message);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"done reaction : {Reaction.Name}");
			}
		}

		
	}
}
