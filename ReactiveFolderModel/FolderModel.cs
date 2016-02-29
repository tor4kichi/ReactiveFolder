using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	public class FolderModel
	{
		public const string REACTION_EXTENTION = ".rf.json";

		public DirectoryInfo Folder { get; private set; }

		private ObservableCollection<FolderReactionModel> _Reactions { get; set; }
		public ReadOnlyObservableCollection<FolderReactionModel> Reactions { get; private set; } 

		public ObservableCollection<FolderModel> _Children { get; private set; }
		public ReadOnlyObservableCollection<FolderModel> Children { get; private set; }

		FolderModel(DirectoryInfo folder)
		{
			Folder = folder;

			_Reactions = new ObservableCollection<FolderReactionModel>();
			Reactions = new ReadOnlyObservableCollection<FolderReactionModel>(_Reactions);
			_Children = new ObservableCollection<FolderModel>();
			Children = new ReadOnlyObservableCollection<FolderModel>(_Children);
		}

		public static FolderModel LoadFolder(DirectoryInfo dir)
		{
			var folderModel = new FolderModel(dir);

			folderModel.UpdateReactionModels();

			folderModel.UpdateChildren();

			return folderModel;
			
		}

		public void UpdateChildren()
		{
			var folders = Folder.EnumerateDirectories();


			// 追加されたフォルダ
			var addFolders = folders
				.Where(x => _Children.All(y => y.Folder.FullName != x.FullName))
				.Select(x => FolderModel.LoadFolder(x));

			foreach (var newChild in addFolders)
			{
				_Children.Add(newChild);
			}


			// 削除されたフォルダ
			var removeFolders = _Children
				.Where(x => folders.All(y => x.Folder.FullName != y.FullName))
				.ToList();

			foreach(var removeFolder in removeFolders)
			{
				_Children.Remove(removeFolder);
			}
		}

		public void UpdateReactionModels()
		{
			var files = Folder.EnumerateFiles($"*{REACTION_EXTENTION}");

			// TODO: 読み込みの最適化
			// ファイル名で既に読まれているアイテムと比較する
			// インスタンスを生成する前に判断するように軽量化したい


			var models = files.Select(fileInfo =>
			{
				FolderReactionModel reaction;
				try
				{
					reaction = FileSerializeHelper.LoadAsync<FolderReactionModel>(fileInfo);
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine("faield Reaction File loading. filepath : " + fileInfo.FullName);
					System.Diagnostics.Debug.WriteLine(e.Message);
					return null;
				}

				var name = fileInfo.Name.Substring(0, fileInfo.Name.Length - FolderModel.REACTION_EXTENTION.Length);

				if (name != reaction.Guid.ToString())
				{
					FolderReactionModel.ResetGuid(reaction);

					var oldFilePath = fileInfo.FullName;
					var renamedPath = MakeReactionSaveFilePath(reaction);
					try
					{
						fileInfo.MoveTo(renamedPath);

						System.Diagnostics.Debug.WriteLine("Reaction Renamed:");
						System.Diagnostics.Debug.WriteLine("from : " + oldFilePath);
						System.Diagnostics.Debug.WriteLine("to   : " + renamedPath);
					}
					catch(Exception e)
					{
						System.Diagnostics.Debug.WriteLine(e.Message);
					}
				}

				return reaction;
			})
			.Where(x => x != null)
			.ToArray();


			// すでに読み込まれていて、Folderにデータが存在しているReactionModelに影響を与えずに
			// Modelsの内容を更新していく。
			var loadedModels = models.Where(x => x != null);


			// 追加されたモデル
			var addTargetModel = loadedModels
				.Where(x => _Reactions.All(y => x.Guid != y.Guid));

			foreach (var model in addTargetModel)
			{
				_Reactions.Add(model);
			}


			// 削除されたモデル
			var removeTargetModels = _Reactions
				.Where(x => loadedModels.All(y => x.Guid != y.Guid))
				.ToList();

			foreach(var removeTarget in removeTargetModels)
			{
				_Reactions.Remove(removeTarget);
			}
		}

		public void AddReaction(FolderReactionModel reaction)
		{
			// Note: 特にインポートした時にGuidが重複しているときの対策
			if (null != FindReaction(reaction.Guid))
			{
				FolderReactionModel.ResetGuid(reaction);
			}

			_Reactions.Add(reaction);

			SaveReaction(reaction);
		}

		public void RemoveReaction(Guid guid)
		{
			var removeTarget = _Reactions.SingleOrDefault(x => x.Guid == guid);
			if (removeTarget == null)
			{
				// 子Folderまではチェックしない
				throw new Exception();
			}

			_Reactions.Remove(removeTarget);

			DeleteReaction(removeTarget);
		}

		public FolderReactionModel ReactionCopyTo(FolderReactionModel target, FolderModel destFolder)
		{
			throw new NotImplementedException("ReactionCopyTo is still implement.");
		}





		public FolderModel FindFolder(string path)
		{
			FolderModel folder = null;
			if (this.Folder.FullName == path)
			{
				folder = this;
			}
			else
			{
				foreach (var child in Children)
				{
					folder = child.FindFolder(path);
					if (folder != null)
					{
						break;
					}
				}
			}

			return folder;
		}


		public bool CanAddFolder(string name)
		{
			return Folder.EnumerateDirectories().All(x => x.Name != name);
		}

		public FolderModel AddFolder(string name)
		{
			var newFolderPath = Path.Combine(this.Folder.FullName, name);

			var newFolderInfo = new DirectoryInfo(newFolderPath);

			if (newFolderInfo.Exists)
			{
				var existFolder = Children.SingleOrDefault(x => x.Folder.FullName == newFolderInfo.FullName);
				if (existFolder == null)
				{
					throw new Exception("????????");
				}

				return existFolder;
			}

			// create
			newFolderInfo.Create();

			var newFolder = FolderModel.LoadFolder(newFolderInfo);

			_Children.Add(newFolder);

			return newFolder;
		}


		/// <summary>
		/// フォルダーを削除します。
		/// 引数で渡されたfolderのフォルダツリー全てを対象に探査します。
		/// </summary>
		/// <param name="folder"></param>
		/// <returns></returns>
		public bool RemoveFolder(FolderModel folder)
		{
			if (_Children.Contains(folder))
			{
				_Children.Remove(folder);

				folder.Folder.Delete(true);

				return true;
			}

			foreach(var child in _Children)
			{
				if (child.RemoveFolder(child))
				{
					return true;
				}
			}

			return false;
		}

		public FolderReactionModel FindReaction(Guid guid)
		{
			var reaction = Reactions.SingleOrDefault(x => x.Guid == guid);
			if (reaction == null)
			{
				foreach(var child in Children)
				{
					reaction = child.FindReaction(guid);

					if (reaction != null) { break; }
				}
			}
			return reaction;
		}

		public FolderModel FindReactionParent(FolderReactionModel model)
		{
			if (Reactions.Contains(model))
			{
				return this;
			}

			foreach (var child in Children)
			{
				var folder = child.FindReactionParent(model);
				if (folder != null) { return folder; }
			}

			return null;
		}

		public FolderModel FindReactionParent(Guid guid)
		{
			if (Reactions.Any(x => x.Guid == guid))
			{
				return this;
			}

			foreach(var child in Children)
			{
				var folder = child.FindReactionParent(guid);
				if (folder != null) { return folder; }
			}

			return null;
		}

		

		public string MakeReactionSaveFilePath(FolderReactionModel reaction)
		{
			return Path.Combine(Folder.FullName, reaction.Guid.ToString() + REACTION_EXTENTION);
		}


		public void SaveReaction(FolderReactionModel reaction)
		{
			var reactionFileName = MakeReactionSaveFilePath(reaction);

			var reactionFileInfo = new FileInfo(reactionFileName);
			if (reactionFileInfo.Exists)
			{
				reactionFileInfo.Delete();
			}

			FileSerializeHelper.Save(reactionFileInfo, reaction);
		}


		private async void DeleteReaction(FolderReactionModel reaction)
		{
			var reactionFileName = MakeReactionSaveFilePath(reaction);

			var reactionFileInfo = new FileInfo(reactionFileName);
			if (reactionFileInfo.Exists)
			{
				await Task.Run(() => reactionFileInfo.Delete());
			}
		}
	}
}
