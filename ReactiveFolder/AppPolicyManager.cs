using ReactiveFolder.Model.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveFolder.Model.Filters;
using System.IO;
using ReactiveFolder.Util;
using ReactiveFolder.Model.AppPolicy;
using System.Collections.ObjectModel;

namespace ReactiveFolder
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
				var policy = FileSerializeHelper.LoadAsync<ApplicationPolicy>(fileInfo);
				factory._Policies.Add(policy);
			}

			return factory;
		}



		private ObservableCollection<ApplicationPolicy> _Policies { get; set; }
		public ReadOnlyObservableCollection<ApplicationPolicy> Policies { get; private set; }

		public DirectoryInfo SaveFolderInfo { get; private set; }


		


		public AppPolicyManager(DirectoryInfo saveFolderInfo)
		{
			SaveFolderInfo = saveFolderInfo;
			_Policies = new ObservableCollection<ApplicationPolicy>();
			Policies = new ReadOnlyObservableCollection<ApplicationPolicy>(_Policies);
		}






		private string MakePolicyFilePath(ApplicationPolicy policy)
		{
			return Path.ChangeExtension(
				Path.Combine(this.SaveFolderInfo.FullName, policy.AppName)
				, ".rfpolicy.json"
				);
		}




		public void SavePolicyFile(ApplicationPolicy policy)
		{
			var filepath = MakePolicyFilePath(policy);

			FileSerializeHelper.Save(filepath, policy);
		}


		public void AddAppPolicy(ApplicationPolicy policy)
		{
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
			var filepath = MakePolicyFilePath(policy);

			var fileInfo = new FileInfo(filepath);

			if (fileInfo.Exists)
			{
				fileInfo.Delete();
			}
		}		
	}
}
