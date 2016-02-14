using System;
using System.ComponentModel;
using System.IO;

namespace ReactiveFolder.Models
{
	public interface IFolderReactionMonitorModel : IDisposable, INotifyPropertyChanged
	{
		FolderModel RootFolder { get; }
		DirectoryInfo ReactionSaveFolder { get; set; }

		FolderModel FindFolder(string path);
		FolderReactionModel FindReaction(Guid guid);
		FolderModel FindReactionParentFolder(Guid guid);
		FolderModel FindReactionParentFolder(FolderReactionModel model);

		TimeSpan DefaultInterval { get; set; }
		void Start();
		void Exit();

		void PauseMonitoring(FolderReactionModel reaction);
		void ResumeMonitoring(FolderReactionModel reaction);
	}

	public static class IFolderReactionMonitorModelHelper
	{
		public static void SaveReaction(this IFolderReactionMonitorModel moonitor, FolderReactionModel reaction)
		{
			var folder = moonitor.FindReactionParentFolder(reaction);
			if (folder != null)
			{
				folder.SaveReaction(reaction);
			}
			else
			{
				// 削除されたリアクション、またはフォルダが削除されている
			}
		}

	}

}