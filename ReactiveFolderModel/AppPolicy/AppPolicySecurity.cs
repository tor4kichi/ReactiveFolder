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

		private ObservableCollection<AppSecurityInfo> _AuthorizedApplicationPathList { get; set; }

		public ReadOnlyObservableCollection<AppSecurityInfo> AuthorizedApplicationPathList { get; private set; }


		public AppPolicySecurity(string savePath)
		{
			SavePath = savePath;

			_AuthorizedApplicationPathList = new ObservableCollection<AppSecurityInfo>();
			AuthorizedApplicationPathList = new ReadOnlyObservableCollection<AppSecurityInfo>(_AuthorizedApplicationPathList);

			Load();
		}


		public bool IsAuthorized(ApplicationPolicy policy)
		{
			var info = new FileInfo(policy.ApplicationPath);
			var app = AuthorizedApplicationPathList.SingleOrDefault(x => x.ApplicationPath == policy.ApplicationPath);
			if (app != null)
			{
				return app.CheckSum == policy.ApplicationCheckSum;
			}
			else
			{
				return false;
			}
		}

		public void AuthorizeApplication(ApplicationPolicy policy, string path)
		{
			var info = new FileInfo(path);
			if (info.Exists)
			{
				UnauthorizeApplication(policy);

				var securityInfo = new AppSecurityInfo(path);
				_AuthorizedApplicationPathList.Add(securityInfo);

				policy.ResetApplicationPath(securityInfo);

				OnPropertyChanged(nameof(AuthorizedApplicationPathList));

				Save();
			}
		}

		public void UnauthorizeApplication(ApplicationPolicy policy)
		{
			if (IsAuthorized(policy))
			{
				var app = _AuthorizedApplicationPathList.SingleOrDefault(x => x.ApplicationPath == policy.ApplicationPath);

				if (app != null)
				{
					_AuthorizedApplicationPathList.Remove(app);

					OnPropertyChanged(nameof(AuthorizedApplicationPathList));

					Save();
				}

				policy.ClearApplicationPath();
			}
		}

		public void ClearAuthorizedApplicationList()
		{
			_AuthorizedApplicationPathList.Clear();
			Save();
		}


		private void Load()
		{
			try
			{
				if (File.Exists(SavePath))
				{
					var list = FileSerializeHelper.LoadAsync<AppSecurityInfo[]>(SavePath);

					foreach (var authorizedApp in list)
					{
						_AuthorizedApplicationPathList.Add(authorizedApp);
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
			var data = _AuthorizedApplicationPathList.ToArray();

			FileSerializeHelper.Save(SavePath, data);
		}
	}

	[DataContract]
	public class AppSecurityInfo
	{
		[DataMember]
		public DateTime Time { get; private set; }

		[DataMember]
		public string CheckSum { get; private set; }

		[DataMember]
		public string ApplicationPath { get; private set; }

		public AppSecurityInfo() { }

		public AppSecurityInfo(string path)
		{
			Time = DateTime.Now;
			ApplicationPath = path;
			CheckSum = CreateCheckSum(ApplicationPath);
		}

		private string CreateCheckSum(string path)
		{
			var fileInfo = new FileInfo(path);
			byte[] hashedBytes;
			using (var readStream = fileInfo.OpenRead())
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
