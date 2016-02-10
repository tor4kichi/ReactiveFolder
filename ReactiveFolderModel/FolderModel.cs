using ReactiveFolder.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model
{
	public class FolderModel
	{

		public DirectoryInfo Folder { get; private set; }

		private ObservableCollection<FolderReactionModel> _Models { get; set; }
		public ReadOnlyObservableCollection<FolderReactionModel> Models { get; private set; } 

		public ObservableCollection<FolderModel> _Children { get; private set; }
		public ReadOnlyObservableCollection<FolderModel> Children { get; private set; }

		FolderModel(DirectoryInfo folder)
		{
			Folder = folder;

			_Models = new ObservableCollection<FolderReactionModel>();
			Models = new ReadOnlyObservableCollection<FolderReactionModel>(_Models);
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
			var files = Folder.EnumerateFiles("*.json")
				.Where(x => x.Name != FolderReactionMonitorModel.MONITOR_SETTINGS_FILENAME);


			var models = files.Select(fileInfo =>
			{
				return FileSerializeHelper.LoadAsync<FolderReactionModel>(fileInfo);
			})
			.ToArray();


			// すでに読み込まれていて、Folderにデータが存在しているReactionModelに影響を与えずに
			// Modelsの内容を更新していく。
			var loadedModels = models.Where(x => x != null);


			// 追加されたモデル
			var addTargetModel = loadedModels
				.Where(x => _Models.All(y => x.Guid != y.Guid));

			foreach (var model in addTargetModel)
			{
				_Models.Add(model);
			}


			// 削除されたモデル
			var removeTargetModels = _Models
				.Where(x => loadedModels.All(y => x.Guid != y.Guid))
				.ToList();

			foreach(var removeTarget in removeTargetModels)
			{
				_Models.Remove(removeTarget);
			}
		}

		public FolderReactionModel AddReaction()
		{
			var reaction = new FolderReactionModel();

			_Models.Add(reaction);

			SaveReaction(reaction);

			return reaction;
		}

		public void RemoveReaction(Guid guid)
		{
			var removeTarget = _Models.SingleOrDefault(x => x.Guid == guid);
			if (removeTarget == null)
			{
				// 子Folderまではチェックしない
				throw new Exception();
			}

			removeTarget.Exit();

			_Models.Remove(removeTarget);

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
				folder.Exit();

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
			var reaction = Models.SingleOrDefault(x => x.Guid == guid);
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
			if (Models.Contains(model))
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
			if (Models.Any(x => x.Guid == guid))
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

		public void Start()
		{
			System.Diagnostics.Debug.WriteLine($"start: {Folder.FullName}");

			foreach (var reaction in Models)
			{
				if (false == reaction.Start())
				{
					System.Diagnostics.Debug.WriteLine($"{reaction.Name}は問題があるためモニタータスクを開始できませんでした。");
				}
			}

			foreach (var child in Children)
			{
				child.Start();
			}

			System.Diagnostics.Debug.WriteLine($"start done: {Folder.FullName}");
		}

		public void Exit()
		{
			foreach (var reaction in Models)
			{
				reaction.Exit();
			}

			foreach (var child in Children)
			{
				child.Exit();
			}
		}



		public void SaveReaction(FolderReactionModel reaction)
		{
			var saveFolder = Folder;

			var reactionFileName = Path.Combine(saveFolder.FullName, reaction.Guid.ToString() + ".json");

			var reactionFileInfo = new FileInfo(reactionFileName);
			if (reactionFileInfo.Exists)
			{
				reactionFileInfo.Delete();
			}

			FileSerializeHelper.Save(reactionFileInfo, reaction);
		}


		private async void DeleteReaction(FolderReactionModel reaction)
		{
			var saveFolder = Folder;

			var reactionFileName = Path.Combine(saveFolder.FullName, reaction.Guid.ToString() + ".json");

			var reactionFileInfo = new FileInfo(reactionFileName);
			if (reactionFileInfo.Exists)
			{
				await Task.Run(() => reactionFileInfo.Delete());
			}
		}
	}
}
