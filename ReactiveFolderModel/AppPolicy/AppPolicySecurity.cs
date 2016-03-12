using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Models.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.AppPolicy
{
	public class AppPolicySecurity : BindableBase
	{
		public string SavePath { get; private set; }

		private ObservableCollection<AppAuthorication> _AppAuthoricationList { get; set; }

		public ReadOnlyObservableCollection<AppAuthorication> AppAuthoricationList { get; private set; }


		public AppPolicySecurity(string savePath)
		{
			SavePath = savePath;

			_AppAuthoricationList = new ObservableCollection<AppAuthorication>();
			AppAuthoricationList = new ReadOnlyObservableCollection<AppAuthorication>(_AppAuthoricationList);

			Load();
		}


		public bool IsAuthorized(string path)
		{
			var alreadyAuth = _AppAuthoricationList.SingleOrDefault(x => x.ApplicationPath == path);
			if (alreadyAuth != null)
			{
				return alreadyAuth.IsValid;
			}
			else
			{
				return false;
			}
		}

		public void AuthorizeApplication(string path)
		{
			var info = new FileInfo(path);
			if (info.Exists)
			{
				var alreadyAuthorized = _AppAuthoricationList.SingleOrDefault(x => x.ApplicationPath == path);
				if (alreadyAuthorized != null)
				{
					alreadyAuthorized.UpdateAuth();

					Save();
				}
				else
				{
					var securityInfo = new AppAuthorication(path);
					_AppAuthoricationList.Add(securityInfo);

					OnPropertyChanged(nameof(AppAuthoricationList));

					Save();
				}
			}
		}

		public void UnauthorizeApplication(string path)
		{
			if (IsAuthorized(path))
			{
				var app = _AppAuthoricationList.SingleOrDefault(x => x.ApplicationPath == path);

				if (app != null)
				{
					_AppAuthoricationList.Remove(app);

					OnPropertyChanged(nameof(AppAuthoricationList));

					Save();
				}
			}
		}

		public void ClearAuthorizedApplicationList()
		{
			_AppAuthoricationList.Clear();

			Save();
		}


		private void Load()
		{
			try
			{
				if (File.Exists(SavePath))
				{
					var list = FileSerializeHelper.LoadAsync<AppAuthorication[]>(SavePath);

					foreach (var authorizedApp in list)
					{
						_AppAuthoricationList.Add(authorizedApp);
					}
				}
			}
			catch
			{
				if (File.Exists(SavePath))
				{
					File.Delete(SavePath);
				}
			}
		}

		private void Save()
		{
			var data = _AppAuthoricationList.ToArray();

			FileSerializeHelper.Save(SavePath, data);
		}
	}

	[DataContract]
	public class AppAuthorication
	{
		[DataMember]
		public DateTime FileLastUpdate { get; private set; }

		[DataMember]
		public string CheckSum { get; private set; }

		[DataMember]
		public string ApplicationPath { get; private set; }



		private FileInfo _FileInfo;
		public FileInfo FileInfo
		{
			get
			{
				return _FileInfo
					?? (_FileInfo = new FileInfo(ApplicationPath));
			}
		}




		public AppAuthorication() { }

		public AppAuthorication(string path)
		{
			ApplicationPath = path;

			UpdateAuth();
		}






		internal void UpdateAuth()
		{
			FileLastUpdate = FileInfo.LastWriteTime;
			CheckSum = CreateCheckSum(FileInfo);
		}






		// TODO: ファイル更新がされていたらCheckSumを再度取得して、異なっていたら承認済みを取り消す

		private bool Validate()
		{
			FileInfo.Refresh();

			if (false == FileInfo.Exists)
			{
				return false;
			}

			if (FileInfo.LastWriteTime != FileLastUpdate)
			{
				var newCheckSum = CreateCheckSum(FileInfo);
				if (CheckSum != newCheckSum)
				{
					return false;
				}
			}

			return true;
		}

		public bool IsValid
		{
			get
			{
				return Validate();
			}
		}






		public static string CreateCheckSum(FileInfo info)
		{
			byte[] hashedBytes;
			using (var readStream = info.OpenRead())
			{
				hashedBytes = SHA256Cng.Create().ComputeHash(readStream);
			}

			return hashedBytes.Aggregate(new StringBuilder(), (sb, x) =>
				sb.Append(x.ToString("x2"))
			)
			.ToString();
		}

	}
}
