using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.AppPolicy
{
	public interface IAppPolicyManager
	{
	
		ReadOnlyObservableCollection<ApplicationPolicy> Policies { get; }

		bool HasAppPolicy(ApplicationPolicy policy);
		void AddAppPolicy(ApplicationPolicy policy);
		void RemoveAppPolicy(ApplicationPolicy policy);

		void SavePolicyFile(ApplicationPolicy policy);

		string PolicyFileExtention { get; }
		string SaveFolderPath { get; }
		string GetSaveFilePath(ApplicationPolicy policy);

	}

	public static class AppPolicyManagerHelper
	{
		public static ApplicationPolicy FromAppGuid(this IAppPolicyManager appPolicyManager, Guid appGuid)
		{
			return appPolicyManager.Policies.SingleOrDefault(x => x.Guid == appGuid);
		}

		public static IEnumerable<ApplicationPolicy> GetPoliciesWithFileFilter(this IAppPolicyManager appPolicyManager, ReactiveFolder.Model.Filters.FileReactiveFilter filter)
		{
			// Filterから出力予定の拡張子集合に対して部分一致するアプリポリシーを取得する
			return appPolicyManager.Policies.Where(x =>
			{
				return filter.FilterWithExtention(x.AcceptExtentions).Count() > 0;
			});
			

		}


	}


}
