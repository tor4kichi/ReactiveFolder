using ReactiveFolder.Models.AppPolicy;
using ReactiveFolder.Models.Util;
using ReactiveFolderStyles.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models
{
	public class InstantActionManager : IInstantActionManager
	{
		public const string INSTANT_ACTION_FILE_EXTENTION = ".rfinstant.json";

		private string _SaveFolder;
		public string SaveFolder
		{
			get
			{
				return _SaveFolder;
			}
			set
			{
				_SaveFolder = value;
				if (false == String.IsNullOrEmpty(_SaveFolder))
				{
					var dir = new DirectoryInfo(_SaveFolder);
					if (false == dir.Exists)
					{
						dir.Create();
					}
				}
			}
		}



		private string _TempSaveFolder;
		public string TempSaveFolder
		{
			get
			{
				return _TempSaveFolder;
			}
			set
			{
				_TempSaveFolder = value;
				if (false == String.IsNullOrEmpty(_TempSaveFolder))
				{
					var dir = new DirectoryInfo(_TempSaveFolder);
					if (false == dir.Exists)
					{
						dir.Create();
					}
				}
			}
		}

		public void Save(InstantActionModel instantAction)
		{
			var serializeData = InstantActionSaveModel.CreateFromInstantActionModel(instantAction);
			var savePath = GetSavePath(instantAction);
			FileSerializeHelper.Save(savePath, serializeData);
		}

		public InstantActionModel Load(string path, IAppPolicyManager appPolicyManager)
		{
			var serializeData = FileSerializeHelper.LoadAsync<InstantActionSaveModel>(path);
			return InstantActionModel.FromSerializedData(serializeData, appPolicyManager);
		}

		public string GetSavePath(InstantActionModel instantAction)
		{
			var fileName = instantAction.Guid.ToString() + INSTANT_ACTION_FILE_EXTENTION;
			return Path.Combine(SaveFolder, fileName);
		}
	}
}
