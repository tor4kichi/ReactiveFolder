using ReactiveFolder.Model.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;

namespace ReactiveFolder.Model.Timings
{
	[DataContract]
	public class FileUpdateReactiveTiming : ReactiveTimingBase
	{
		// TODO: ソースファイルの更新にフックする



		[DataMember]
		public List<PreservedFileInfo> SourceFiles { get; private set; }

		public FileUpdateReactiveTiming()
		{
			SourceFiles = new List<PreservedFileInfo>();

		}


		public override IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev)
		{
			// ファイルやフォルダが更新された作成されていた場合にObservableシーケンスを後続に流す
			return prev
				.Where(payload => FileIsNeedUpdate(payload))
				.Do(payload => UpdateTargetFile(payload));
		}



		protected override ValidationResult InnerValidate()
		{
			return ValidationResult.ValidResult;
		}




		public override void Initialize(DirectoryInfo workDir)
		{
			// TODO: SourceFilesに追加したファイルが削除されていないかチェック
			var removedItems = new List<PreservedFileInfo>();
			foreach(var file in SourceFiles)
			{
				if (false == File.Exists(file.Path) && 
					false == Directory.Exists(file.Path))
				{
					removedItems.Add(file);
				}
			}

			foreach(var removeItem in removedItems)
			{
				SourceFiles.Remove(removeItem);
			}
		}



		public bool FileIsNeedUpdate(ReactiveStreamContext payload)
		{
			return true;
			/*
			var fileAlreadyExist = SourceFiles.Any(x =>
			{
				return x.Path == payload.SourcePath;
			});

			if (fileAlreadyExist)
			{
				// すでに同一のPathが登録されていれば
				// 更新時間をチェックして更新が必要か判断する
				var needUpdate = SourceFiles.Any(x =>
				{
					if (payload.SourcePath == x.Path)
					{
						var lastUpdateTime = File.GetLastWriteTime(payload.SourcePath);
						return lastUpdateTime > x.UpdateTime;
					}
					return false;
				});

				return needUpdate;
			}
			else
			{
				// まだPathが登録されていない場合は更新が必要
				return true;
			}
			*/
		}

		void UpdateTargetFile(ReactiveStreamContext payload)
		{
			var fileAlreadyExist = SourceFiles.Any(x =>
			{
				return x.Path == payload.SourcePath;
			});

			if (false == fileAlreadyExist)
			{
				SourceFiles.Add(new PreservedFileInfo(payload.SourcePath));
			}
		}

		
	}

	public struct PreservedFileInfo
	{
		public string Path { get; set; }
		public DateTime UpdateTime { get; set; }

		public PreservedFileInfo(string path)
		{
			Path = path;
			if (File.Exists(path))
			{
				var fileInfo = new FileInfo(Path);
				UpdateTime = fileInfo.LastWriteTime;
			}

			UpdateTime = DateTime.Now;
		}
	}
}
