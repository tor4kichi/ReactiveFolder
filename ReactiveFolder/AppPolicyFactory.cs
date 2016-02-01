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

namespace ReactiveFolder
{
	// Note: Policy のリネームは基本的にサポートされない。
	// 

	public class AppPolicyFactory : IAppPolicyFactory
	{
		public const string APP_POLICY_EXTENTION = ".rfpolicy.json";




		public static AppPolicyFactory CreateNew(DirectoryInfo saveFolderInfo)
		{
			if (!saveFolderInfo.Exists)
			{
				saveFolderInfo.Create();
			}

			return new AppPolicyFactory(saveFolderInfo);
		}

		public static AppPolicyFactory Load(DirectoryInfo saveFolderInfo)
		{
			var factory = new AppPolicyFactory(saveFolderInfo);

			foreach (var fileInfo in saveFolderInfo.EnumerateFiles($"*{APP_POLICY_EXTENTION}"))
			{
				var policy = FileSerializeHelper.LoadAsync<ApplicationPolicy>(fileInfo);
				factory.Policies.Add(policy.AppName, policy);
			}

			return factory;
		}






		public Dictionary<string, ApplicationPolicy> Policies { get; private set; }

		public DirectoryInfo SaveFolderInfo { get; private set; }


		


		public AppPolicyFactory(DirectoryInfo saveFolderInfo)
		{
			SaveFolderInfo = saveFolderInfo;
			Policies = new Dictionary<string, ApplicationPolicy>();
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
			Policies.Add(policy.AppName, policy);

			SavePolicyFile(policy);
		}





		public void RemoveAppPolicy(ApplicationPolicy policy)
		{
			Policies.Remove(policy.AppName);

			DeletePolicyFile(policy);
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





		public ApplicationPolicy FromAppName(string name)
		{
			ApplicationPolicy policy;
			if (Policies.TryGetValue(name, out policy))
			{
				System.Diagnostics.Debug.WriteLine("not exist ApplicationPolicy. name = " + name);
			}

			return policy;
		}

		public IEnumerable<ApplicationPolicy> GetPolicies()
		{
			return Policies.Values;
		}

		public IEnumerable<ApplicationPolicy> GetPoliciesWithFilter(FileReactiveFilter filter)
		{
			// filterのFileFilterPatternsを全て保持しているに対してpolicyを返す
			return Policies.Values.Where(x =>
			{
				return filter.FileFilterPatterns.All(y => x.PathFilterPartterns.Contains(y));
			});
		}

		
	}
}
