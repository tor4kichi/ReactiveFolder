using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;

namespace ReactiveFolder.Models.Timings
{
	

	[DataContract]
	public class FileUpdateReactiveTiming : ReactiveTimingBase
	{
		public static IFileUpdateRecordManager FileUpdateRecordManager { get; private set; }

		public static void SetFileUpdateRecordManager(IFileUpdateRecordManager manager)
		{
			FileUpdateRecordManager = manager;
		}




		public List<FolderItemUpdateRecord> SourceFiles { get; private set; }

		public FileUpdateReactiveTiming()
		{

		}


		public override void Execute(ReactiveStreamContext context)
		{
			// ファイルやフォルダが更新された作成されていた場合にObservableシーケンスを後続に流す
			if (false == CheckItemNeedUpdate(context))
			{ 
				context.Done();
			}
		}



		protected override ValidationResult InnerValidate()
		{
			return ValidationResult.ValidResult;
		}




		public override void Initialize(DirectoryInfo workDir)
		{
			if (workDir == null) { return; }



			SourceFiles = FileUpdateRecordManager.GetRecord(this.ParentReactionModel);

			var removedItems = new List<FolderItemUpdateRecord>();
			foreach(var file in SourceFiles)
			{				
				if (false == file.IsSourceItemExist)
				{
					removedItems.Add(file);
				}
			}

			foreach(var removeItem in removedItems)
			{
				SourceFiles.Remove(removeItem);
			}
		}



		public bool CheckItemNeedUpdate(ReactiveStreamContext payload)
		{
			var previewProcessedFile = SourceFiles.SingleOrDefault(x =>
			{
				return x.Source.Path == payload.OriginalPath;
			});

			return previewProcessedFile?.IsNeedUpdate ?? true;
		}

		public void OnContextComplete(ReactiveStreamContext context)
		{
			// 対象アイテムの更新を記録する
			PreserveItemOnFirstUpdate(context.OriginalPath, context.OutputPath);
		}

		public void OnContextFailed()
		{

		}

		public void OnCompleteReaction()
		{
			FileUpdateRecordManager.SaveRecord(this.ParentReactionModel, SourceFiles);
			SourceFiles = null;
		}


		private void PreserveItemOnFirstUpdate(string sourcePath, string destPath)
		{
			var name = Path.GetFileName(sourcePath);

			var fileAlreadyExist = SourceFiles.Any(x =>
			{
				return x.Source.Name == name;
			});

			if (false == fileAlreadyExist)
			{
				IFolderItem sourceItem = FolderItemHelper.FromPath(sourcePath);
				IFolderItem destItem = FolderItemHelper.FromPath(destPath);

				SourceFiles.Add(new FolderItemUpdateRecord(sourceItem, destItem));
			}
		}
	}

	[DataContract]
	public class FolderItemUpdateRecord
	{
		[DataMember]
		public IFolderItem Source { get; private set; }

		[DataMember]
		public IFolderItem Dest { get; private set; }


		public FolderItemUpdateRecord()
		{

		}

		public FolderItemUpdateRecord(IFolderItem source, IFolderItem dest)
		{
			Source = source;
			Dest = dest;
		}

		public bool IsSourceItemExist
		{
			get
			{
				return Source.IsExist;
			}
		}

		public bool IsNeedUpdate
		{
			get
			{
				return Source.IsUpdated || false == Dest.IsExist;
			}
		}
	}

	public interface IFolderItem
	{
		FolderItemType ItemType { get; }
		string Path { get; }
		DateTime PreviewUpdateTime { get; }


		string Name { get; }
		bool IsExist { get; }
		bool IsUpdated { get; }
	}

	[DataContract]
	public class FileItem : IFolderItem
	{
		[DataMember]
		public FileInfo FileInfo { get; private set; }

		[DataMember]
		public DateTime PreviewUpdateTime { get; private set; }


		public FileItem()
		{

		}

		public FileItem(string path)
			: this(new FileInfo(path))
		{

		}

		public FileItem(FileInfo info)
		{
			FileInfo = info;

			PreviewUpdateTime = DateTime.Now;
		}

		public bool IsExist
		{
			get
			{
				FileInfo.Refresh();
				return FileInfo.Exists;
			}
		}

		public bool IsUpdated
		{
			get
			{
				FileInfo.Refresh();

				return FileInfo.LastWriteTime > PreviewUpdateTime;
			}
		}

		public FolderItemType ItemType
		{
			get
			{
				return FolderItemType.File;
			}
		}

		public string Path
		{
			get
			{
				return FileInfo.FullName;
			}
		}

		public string Name
		{
			get
			{
				return FileInfo.Name;
			}
		}
		
	}

	[DataContract]
	public class FolderItem : IFolderItem
	{
		[DataMember]
		public DirectoryInfo FolderInfo { get; private set; }

		[DataMember]
		public DateTime PreviewUpdateTime { get; private set; }

		public FolderItem()
		{

		}

		public FolderItem(string path)
			: this(new DirectoryInfo(path))
		{

		}

		public FolderItem(DirectoryInfo info)
		{
			FolderInfo = info;

			PreviewUpdateTime = DateTime.Now;
		}

		public bool IsExist
		{
			get
			{
				FolderInfo.Refresh();
				return FolderInfo.Exists;
			}
		}

		public bool IsUpdated
		{
			get
			{
				// TODO: フォルダ内のアイテム全ての更新をチェックしたほうがいい？
				FolderInfo.Refresh();

				return FolderInfo.LastWriteTime > PreviewUpdateTime;
			}
		}

		public FolderItemType ItemType
		{
			get
			{
				return FolderItemType.Folder;
			}
		}

		public string Path
		{
			get
			{
				return FolderInfo.FullName;
			}
		}

		public string Name
		{
			get
			{
				return FolderInfo.Name;
			}
		}

	}


	public static class FolderItemHelper
	{
		public static IFolderItem FromPath(string path)
		{
			FolderItemType sourceItemType = FolderItemTypeHelper.FromPath(path);

			switch (sourceItemType)
			{
				case FolderItemType.File:
					return new FileItem(path);
				case FolderItemType.Folder:
					return new FolderItem(path);
				default:
					throw new Exception();
			}
		}

	}
}
