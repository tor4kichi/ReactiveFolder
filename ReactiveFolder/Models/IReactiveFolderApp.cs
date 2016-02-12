namespace ReactiveFolder.Models
{
	public interface IReactiveFolderApp
	{
		AppPolicyManager AppPolicyManager { get; }
		FolderReactionMonitorModel ReactionMonitor { get; }
		ReactiveFolderGlobalSettings Settings { get; }

		void LoadGlobalSettings();
		void SaveGlobalSettings();
	}
}