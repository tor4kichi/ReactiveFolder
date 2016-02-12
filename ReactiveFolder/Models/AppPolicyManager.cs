using ReactiveFolder.Models.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Models.Filters;
using System.IO;
using ReactiveFolder.Models.Util;
using ReactiveFolder.Models.AppPolicy;
using System.Collections.ObjectModel;

namespace ReactiveFolder.Models
{
	// Note: Policy のリネームは基本的にサポートされない。
	// 

	public class AppPolicyManager : IAppPolicyManager
	{
		public const string APP_POLICY_EXTENTION = ".rfpolicy.json";

		


		public static AppPolicyManager CreateNew(DirectoryInfo saveFolderInfo)
		{
			if (!saveFolderInfo.Exists)
			{
				saveFolderInfo.Create();
			}

			return new AppPolicyManager(saveFolderInfo);
		}

		public static AppPolicyManager Load(DirectoryInfo saveFolderInfo)
		{
			var factory = new AppPolicyManager(saveFolderInfo);

			foreach (var fileInfo in saveFolderInfo.EnumerateFiles($"*{APP_POLICY_EXTENTION}"))
			{
				try
				{
					var policy = FileSerializeHelper.LoadAsync<ApplicationPolicy>(fileInfo);
					factory._Policies.Add(policy);
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine("faield app policy loading. filepath : " + fileInfo.FullName);
					System.Diagnostics.Debug.WriteLine(e.Message);
				}
			}

			return factory;
		}



		private ObservableCollection<ApplicationPolicy> _Policies { get; set; }
		public ReadOnlyObservableCollection<ApplicationPolicy> Policies { get; private set; }

		public DirectoryInfo SaveFolderInfo { get; private set; }


		public string SaveFolderPath
		{
			get
			{
				return SaveFolderInfo.FullName;
			}
		}

		public string PolicyFileExtention
		{
			get
			{
				return APP_POLICY_EXTENTION;
			}
		}


		public AppPolicyManager(DirectoryInfo saveFolderInfo)
		{
			SaveFolderInfo = saveFolderInfo;
			_Policies = new ObservableCollection<ApplicationPolicy>();
			Policies = new ReadOnlyObservableCollection<ApplicationPolicy>(_Policies);
		}






		public string GetSaveFilePath(ApplicationPolicy policy)
		{
			return Path.ChangeExtension(
				Path.Combine(this.SaveFolderInfo.FullName, policy.Guid.ToString())
				, ".rfpolicy.json"
				);
		}




		public void SavePolicyFile(ApplicationPolicy policy)
		{
			var filepath = GetSaveFilePath(policy);

			FileSerializeHelper.Save(filepath, policy);
		}


		/// <summary>
		/// comperer ApplicationPolicy.Guid
		/// </summary>
		/// <param name="policy"></param>
		/// <returns>if same Guid ApplicationPolicy then return true.</returns>
		public bool HasAppPolicy(ApplicationPolicy policy)
		{
			return _Policies.Any(x => x.Guid == policy.Guid);
		}

		public void AddAppPolicy(ApplicationPolicy policy)
		{
			if (_Policies.Any(x => x.Guid == policy.Guid))
			{
				throw new Exception("Already exist ApplicationPolicy name: " + policy.AppName + " guid:" + policy.Guid);
			}


			_Policies.Add(policy);

			SavePolicyFile(policy);
		}





		public void RemoveAppPolicy(ApplicationPolicy policy)
		{
			if (_Policies.Remove(policy))
			{
				DeletePolicyFile(policy);
			}
		}


		private void DeletePolicyFile(ApplicationPolicy policy)
		{
			var filepath = GetSaveFilePath(policy);

			var fileInfo = new FileInfo(filepath);

			if (fileInfo.Exists)
			{
				fileInfo.Delete();
			}
		}		

	}
}
