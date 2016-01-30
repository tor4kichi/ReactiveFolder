using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace ReactiveFolder.Model
{
	// Note: アクションはフォルダ→フォルダの射影を行わない
	// あるのはフォルダ→ファイルまたはファイル→ファイルの２パターンのみ


	public class ReactiveStreamContext
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

		/// <summary>
		/// IStreamContextUpdater
		/// </summary>
		public DirectoryInfo TempOutputFolder { get; private set; }


		public ReactiveStreamStatus Status { get; private set; }

		public ReactiveStreamContext(DirectoryInfo dir, string itempath, bool protecteOriginal = true)
		{
			WorkFolder = dir;
			IsProtectOriginal = protecteOriginal;

			OriginalPath = itempath;

			SourcePath = OriginalPath;

			TempOutputFolder = GenerateTempOutputFolder();

			Status = ReactiveStreamStatus.Running;
		}




		public string Name
		{
			get
			{
				return Path.GetFileNameWithoutExtension(OriginalPath);
			}
		}

		public void Update(IStreamContextUpdater updater)
		{
			// TODO: OutputPathにすでにファイルが存在していたら
			//			OutputPath = GenerateTempFilePath();

			if (false == IsRunnning)
			{
				return;
			}

			// SourcePathのファイルが存在しなかったら
			var outputType = updater.Update(SourcePath, TempOutputFolder);

			// ファイルまたはフォルダの名前部分だけを抽出
			var sourceItemName = Path.GetFileNameWithoutExtension(SourcePath);


			switch (outputType)
			{
				case OutputItemType.File:
					var processedFileInfo = TempOutputFolder.EnumerateFiles().SingleOrDefault(x => sourceItemName == Path.GetFileNameWithoutExtension(x.Name));

					// ouptutFolderに作成されたアイテムをチェックする
					if (processedFileInfo != null && processedFileInfo.Exists)
					{
						SourcePath = processedFileInfo.FullName;
					}
					else
					{
						// TODO: 出力先フォルダにアイテムが追加されていないよ
					}

//					if (File.Exists(SourcePath) && processedFileInfo.Extension != Path.GetExtension(SourcePath))
//					{
						// ファイル名は同じで拡張子が変更されたよ
//					}

					break;
				case OutputItemType.Folder:
					// TODO: フォルダの更新をチェック
					var foldername = Path.GetFileName(SourcePath);
					var processedFolderInfo = TempOutputFolder.EnumerateDirectories().SingleOrDefault(x => x.Name == foldername);

					if (processedFolderInfo != null && processedFolderInfo.Exists)
					{
						SourcePath = processedFolderInfo.FullName;
					}
					else
					{
						// TODO: 出力先フォルダに作業後のフォルダが
					}
					break;
				case OutputItemType.Failed:
					// 正常系の失敗

					// 失敗時の挙動を選択
					// 1. このアップデートをスキップして実行する。Finalizeも実行する。（継続性重視）
					// 2. このアップデート以降すべて失敗とする。Finalizeも失敗させる。（誠実さ重視）

					// 失敗したことがちゃんと伝わらないとユーザー側もアプリ側も改善するためのアクションを起こせない。
					// よって 2 の誠実さ重視の方針でいく。

					Status = ReactiveStreamStatus.Failed;
					break;
				default:
					break;
			}
		}

		private DirectoryInfo GenerateTempOutputFolder()
		{
			// TODO: アプリ空間のテンポラリフォルダに仮フォルダを作成して返す
			// Context上に一つあれば十分かも
			return new DirectoryInfo(
				Path.Combine(
					Path.GetTempPath(),
					"ReactiveFolder/")
				);
		}

		public string Finalize(IStreamContextFinalizer finalizer)
		{
			if (false == IsRunnning)
			{
				return null;
			}

			// やること
			// アウトプットしたいファイルまたはフォルダをNameに変更した上でDestinationFolderに移動させる

			// 例外状況
			// DestinationFolderとWorkFolderが同じ場合、かつ
			// 拡張子を含むOriginalのファイル名とOutputPathのファイル名が同じ場合、かつ
			// IsProtecteOriginalがtrueのときに
			// Destinationへの配置が実行できない状況が生まれる



			// 検証が必要：フォルダでも同様に配置不可な状況が検知できるか

			Status = ReactiveStreamStatus.Finalizing;

			string outputPath = null;

			try
			{
				var destFolder = finalizer.GetDestinationFolder();
				if (this.WorkFolder.FullName == destFolder.FullName &&
				Path.GetFileName(OriginalPath) == Path.GetFileName(SourcePath) &&
				IsProtectOriginal == true
				)
				{
					// Finalizeに失敗？
					outputPath = null;
				}
				else
				{
					// OutputPathで指定されたファイルをfinalizer.DestinationFolderにコピーする
					// コピーする前にファイル名をNameに変更する（拡張子はOutputPathのものを引き継ぐ）

					string outputName = Path.GetFileNameWithoutExtension(OriginalPath);
					if (false == String.IsNullOrWhiteSpace(finalizer.OutputName))
					{
						outputName = finalizer.OutputName;
					}



					if (IsFile)
					{
						outputPath = FinalizeFile(outputName, destFolder);
					}
					else
					{
						outputPath = FinalizeFolder(outputName, destFolder);
					}

				}
			}
			catch(Exception e)
			{
				Status = ReactiveStreamStatus.Failed;
			}
			finally
			{
				// 必ず仮出力用フォルダはリセットしておく
				TempOutputFolder.Delete(true);
				TempOutputFolder = GenerateTempOutputFolder();

				if (outputPath != null)
				{
					Status = ReactiveStreamStatus.Completed;
				}
				else
				{
					Status = ReactiveStreamStatus.Failed;
				}
			}

			return outputPath;
		}


		private string FinalizeFile(string outputName, DirectoryInfo destFolder)
		{
			
			var outputFileInfo = new FileInfo(SourcePath);
			if (false == outputFileInfo.Exists)
			{
				return null;
			}

			var extention = Path.GetExtension(SourcePath);

			var finalizeFilePath = Path.Combine(
				destFolder.FullName,
				Path.ChangeExtension(
					outputName,
					extention
				)
			);

			try
			{
				outputFileInfo.CopyTo(finalizeFilePath);
				return finalizeFilePath;
			}
			catch (Exception e)
			{
				// TODO: 
			}

			return null;
		}

		private string FinalizeFolder(string outputName, DirectoryInfo destFolder)
		{
			var outputFolderInfo = new DirectoryInfo(SourcePath);
			if (false == outputFolderInfo.Exists)
			{
				return null;
			}


			var finalizeFolderPath = Path.Combine(
				destFolder.FullName,
				outputName
			);

			var finalizeFolderInfo = new DirectoryInfo(finalizeFolderPath);
			try
			{
				if (finalizeFolderInfo.Exists)
				{
					finalizeFolderInfo.Delete();
				}

				outputFolderInfo.MoveTo(finalizeFolderPath);
				return finalizeFolderPath;
			}
			catch(Exception e)
			{
				// TODO: 
			}

			return null;
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
		public bool IsFinalizing { get { return Status == ReactiveStreamStatus.Finalizing; } }

	}

	public enum ReactiveStreamStatus
	{
		Running,
		Finalizing,
		Completed,

		Failed,
	}
}
