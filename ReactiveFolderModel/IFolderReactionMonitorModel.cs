using System;
using System.ComponentModel;
using System.IO;

namespace ReactiveFolder.Models
{
	public interface IFolderReactionMonitorModel : IDisposable, INotifyPropertyChanged
	{
		TimeSpan DefaultInterval { get; set; }
		FolderModel RootFolder { get; }
		DirectoryInfo SaveFolder { get; set; }

		void Exit();
		FolderModel FindFolder(string path);
		FolderReactionModel FindReaction(Guid guid);
		FolderModel FindReactionParentFolder(Guid guid);
		FolderModel FindReactionParentFolder(FolderReactionModel model);
		void Save();
		void SaveReaction(FolderReactionModel reaction);
		void SaveSettings();
		void Start();
	}
}