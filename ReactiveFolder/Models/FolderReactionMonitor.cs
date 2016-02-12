using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings.Extensions;
using ReactiveFolder.Models;
using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

	
		public const string MONITOR_SETTINGS_FILENAME = "settings.json";

		private DirectoryInfo _MonitorSettingsSaveFolder;
		public DirectoryInfo MonitorSettingsSaveFolder
		{
			get
			{
				return _MonitorSettingsSaveFolder;
			}
			set
			{
				if (false == value.Exists)
				{
					value.Create();
				}

				_MonitorSettingsSaveFolder = value;
			}
		}

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
			ReactionSaveFolder = saveFolder;

			DefaultInterval = TimeSpan.FromMinutes(15);

			InitializeReactions();
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

		// TODO: 再帰的にフォルダをめぐってFolderModelを構築する

		private void InitializeReactions()
		{
			RootFolder = FolderModel.LoadFolder(ReactionSaveFolder);
		}




		#region Monitor Manage

		public void Start()
		{
			// 既に走っている監視処理を終了させる
			Exit();


			RootFolder.Start();
		}



		public void Exit()
		{
			RootFolder.Exit();
		}

		public void Dispose()
		{
			Exit();
		}




		#endregion


	}
}
