using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace ReactiveFolder.Models
{
	// Note: アクションはフォルダ→フォルダの射影を行わない
	// あるのはフォルダ→ファイルまたはファイル→ファイルの２パターンのみ

	// Note: Contextは

	public class ReactiveStreamContext : IDisposable
	{
		/// <summary>
		/// <para>Default is true.</para>
		/// <para>オリジナルのファイル/フォルダを保護する・保護しないのフラグです。</para>
		/// <para>trueの場合、入出力フォルダが同一かつ拡張子を含むファイル名が同一の場合に出力が失敗します。</para>
		/// </summary>
		public bool IsProtectOriginal { get; set; }


		/// <summary>
		/// <para>作業元となるフォルダ情報です。</para>
		/// <para>ReactiveFolderGroupModelから提供されます。</para>
		/// </summary>
		public DirectoryInfo WorkFolder { get; private set; }


		/// <summary>
		/// <para>WorkFolder上に存在する作業対象のファイル/フォルダへの絶対パスです。</para>
		/// </summary>
		public string OriginalPath { get; private set; }

		/// <summary>
		/// IStreamContextUpdaterに提供される元ファイル情報です。
		/// IStreamContextUpdaterによって加工されたファイル/フォルダへのパスです。
		/// IStreamContextUpdaterによる加工を受けていない場合、OriginalPathと同じパスを示します。
		/// </summary>
		public string SourcePath { get; private set; }


		public string OutputPath { get; private set; }

		/// <summary>
		/// IStreamContextUpdater
		/// </summary>
		public DirectoryInfo _TempOutputFolder;
		public DirectoryInfo TempOutputFolder
		{
			get
			{
				return _TempOutputFolder
					?? (_TempOutputFolder = GenerateTempOutputFolder());
			}
		}


		public ReactiveStreamStatus Status { get; private set; }

		public int Index { get; private set; }

		public List<string> FailedMessage { get; private set; }
		public Exception FailedCuaseException { get; private set; }


		public ReactiveStreamContext(DirectoryInfo dir, string itempath, int index = -1, bool protecteOriginal = true)
		{
			WorkFolder = dir;
			IsProtectOriginal = protecteOriginal;

			OriginalPath = itempath;

			SourcePath = OriginalPath;

			Status = ReactiveStreamStatus.Running;
		}



		public void Dispose()
		{
			CleanupTempOutputFolder();
		}



		public string Name
		{
			get
			{
				return Path.GetFileNameWithoutExtension(OriginalPath);
			}
		}

		

		private DirectoryInfo GenerateTempOutputFolder()
		{
			// TODO: アプリ空間のテンポラリフォルダに仮フォルダを作成して返す
			// Context上に一つあれば十分かも
			var tempFolderName = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
			var dir = new DirectoryInfo(
				Path.Combine(
					this.WorkFolder.FullName,
					tempFolderName)
				);
			if (false == dir.Exists)
			{
				dir.Create();
			}

			return dir;
		}




		public void SetNextWorkingPath(string workPath)
		{
			SourcePath = workPath;
		}

		public void Failed(string message, Exception e = null)
		{
			Status = ReactiveStreamStatus.Failed;

			// TODO: 
		}

		public void Complete(string outputItemPath)
		{
			if (String.IsNullOrWhiteSpace(outputItemPath))
			{
				Failed("invalid outputItemPath");
				return;
			}

			OutputPath = outputItemPath;
			Status = ReactiveStreamStatus.Completed;
		}

		public void Done()
		{
			Status = ReactiveStreamStatus.Completed;
		}

		public void CleanupTempOutputFolder()
		{
			// 必ず仮出力用フォルダはリセットしておく
			if (TempOutputFolder != null)
			{
				TempOutputFolder.Refresh();
				if (TempOutputFolder.Exists)
				{
					TempOutputFolder.Delete(true);
					_TempOutputFolder = null;
				}
			}
		}

		

		public bool IsFile
		{
			get
			{
				return new FileInfo(SourcePath).Exists;
			}
		}

		public bool IsFailed { get { return Status == ReactiveStreamStatus.Failed; } }
		public bool IsCompleted { get { return Status == ReactiveStreamStatus.Completed; } }
		public bool IsRunnning { get { return Status == ReactiveStreamStatus.Running; } }

	}

	public enum ReactiveStreamStatus
	{
		Running,
		Completed,
		Failed,
	}
}
