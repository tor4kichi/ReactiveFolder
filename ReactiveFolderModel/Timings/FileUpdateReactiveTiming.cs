using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace ReactiveFolder.Model.Timings
{
	public class FileUpdateReactiveTiming : ReactiveTimingBase
	{
		private List<PreservedFileInfo> Files;

		public FileUpdateReactiveTiming()
		{
			Files = new List<PreservedFileInfo>();

		}

		public override IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev)
		{
			// ファイルやフォルダが更新された作成されていた場合にObservableシーケンスを後続に流す
			return prev.Where(payload => FileIsNeedUpdate(payload));
		}



		public override ValidationResult Validate()
		{
			return ValidationResult.Valid;
		}




		public override void Initialize(DirectoryInfo workDir)
		{
			Files.AddRange(workDir.GetFiles().Select(x => new PreservedFileInfo(x.FullName)));
			Files.AddRange(workDir.GetDirectories().Select(x => new PreservedFileInfo(x.FullName)));
		}





		public bool FileIsNeedUpdate(ReactiveStreamContext payload)
		{
			var fileAlreadyExist = Files.Any(x =>
			{
				return x.Path == payload.SourcePath;
			});

			if (fileAlreadyExist)
			{
				// すでに同一のPathが登録されていれば
				// 更新時間をチェックして更新が必要か判断する
				var needUpdate = Files.Any(x =>
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
				// まだPathが登録されていない場合は
				// 新しく作成して、更新するよう返す
				Files.Add(new PreservedFileInfo(payload.SourcePath));

				return true;
			}
		}

		
	}

	struct PreservedFileInfo
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
