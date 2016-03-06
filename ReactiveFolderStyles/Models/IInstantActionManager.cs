using ReactiveFolder.Models;
using ReactiveFolder.Models.AppPolicy;

namespace ReactiveFolderStyles.Models
{
	public interface IInstantActionManager
	{
		string SaveFolder { get; set; }
		string TempSaveFolder { get; set; }

		string GetSavePath(InstantActionModel instantAction);

		void Save(InstantActionModel instantAction);
		InstantActionModel Load(string path, IAppPolicyManager appPolicyManager);
	}
}