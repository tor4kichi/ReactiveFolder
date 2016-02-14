using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.Actions
{
	public abstract class ReactiveActionBase : ReactiveStraightStreamBase, IFolderItemOutputer
	{
		abstract public FolderItemType InputItemType { get; }
		abstract public FolderItemType OutputItemType { get; }

		abstract public IEnumerable<string> GetFilters();



		abstract public void Update(string sourcePath, DirectoryInfo destFolder);

		public override void Execute(ReactiveStreamContext context)
		{

			// TODO: OutputPathにすでにファイルが存在していたら
			//			OutputPath = GenerateTempFilePath();


			// TODO: SourcePathのファイルが存在しなかったら終了

			var sourcePath = context.SourcePath;
			var tempOutputFolder = context.TempOutputFolder;
			
			try
			{
				Update(context.SourcePath, tempOutputFolder);
			}
			catch (Exception e)
			{
				context.Failed("ReactiveStreamFailed on Update", e);
				return;
			}

			// ファイルまたはフォルダの名前部分だけを抽出
			var sourceItemName = Path.GetFileNameWithoutExtension(context.SourcePath);


			switch (OutputItemType)
			{
				case FolderItemType.File:
					var outputFolderFiles = tempOutputFolder.EnumerateFiles();
					var processedFileInfo = outputFolderFiles.SingleOrDefault(x =>
					{
						return sourceItemName == Path.GetFileNameWithoutExtension(x.Name);
					});

					// ouptutFolderに作成されたアイテムをチェックする
					if (processedFileInfo != null && processedFileInfo.Exists)
					{
						sourcePath = processedFileInfo.FullName;
					}
					else
					{
						// TODO: 出力先フォルダにアイテムが追加されていないよ
						context.Failed("not found processed file: " + processedFileInfo.FullName);
					}

					//					if (File.Exists(SourcePath) && processedFileInfo.Extension != Path.GetExtension(SourcePath))
					//					{
					// ファイル名は同じで拡張子が変更されたよ
					//					}

					break;
				case FolderItemType.Folder:
					// TODO: フォルダの更新をチェック
					var foldername = Path.GetFileName(sourcePath);
					var processedFolderInfo = tempOutputFolder.EnumerateDirectories().SingleOrDefault(x => x.Name == foldername);

					if (processedFolderInfo != null && processedFolderInfo.Exists)
					{
						sourcePath = processedFolderInfo.FullName;
					}
					else
					{
						// TODO: 出力先フォルダに作業後のフォルダが
						context.Failed("not found processed folder: " + processedFolderInfo.FullName);
					}
					break;
				//				case FolderItemType.Failed:
				// 正常系の失敗

				// 失敗時の挙動を選択
				// 1. このアップデートをスキップして実行する。Finalizeも実行する。（継続性重視）
				// 2. このアップデート以降すべて失敗とする。Finalizeも失敗させる。（誠実さ重視）

				// 失敗したことがちゃんと伝わらないとユーザー側もアプリ側も改善するためのアクションを起こせない。
				// よって 2 の誠実さ重視の方針でいく。

				//break;
				default:
					break;
			}

			context.SetNextWorkingPath(sourcePath);
		}

	}


}
