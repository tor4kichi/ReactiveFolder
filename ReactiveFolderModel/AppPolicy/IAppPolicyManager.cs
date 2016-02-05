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

		void AddAppPolicy(ApplicationPolicy policy);
		void RemoveAppPolicy(ApplicationPolicy policy);

		void SavePolicyFile(ApplicationPolicy policy);
	}

	public static class AppPolicyManagerHelper
	{
		public static ApplicationPolicy FromAppName(this IAppPolicyManager appPolicyManager, string appName)
		{
			return appPolicyManager.Policies.SingleOrDefault(x => x.AppName == appName);
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
