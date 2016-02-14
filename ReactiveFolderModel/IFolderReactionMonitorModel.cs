using System;
using System.ComponentModel;
using System.IO;

namespace ReactiveFolder.Models
{
	public interface IFolderReactionMonitorModel : IDisposable, INotifyPropertyChanged
	{
		TimeSpan DefaultInterval { get; set; }
		DirectoryInfo ReactionSaveFolder { get; set; }
		FolderModel RootFolder { get; }

		FolderModel FindFolder(string path);
		FolderReactionModel FindReaction(Guid guid);
		FolderModel FindReactionParentFolder(Guid guid);
		FolderModel FindReactionParentFolder(FolderReactionModel model);
		void StartAllReactionMonitoring();
		void StartMonitoring(FolderReactionModel reaction);
		void StopAllReactionMonitoring();
		void StopMonitoring(FolderReactionModel reaction);
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