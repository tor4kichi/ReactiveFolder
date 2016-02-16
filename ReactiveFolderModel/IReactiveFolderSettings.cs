namespace ReactiveFolder.Models
{
	public interface IReactiveFolderSettings
	{
		string AppPolicySaveFolder { get; set; }
		int DefaultMonitorIntervalSeconds { get; set; }
		string ReactionSaveFolder { get; set; }
		string UpdateRecordSaveFolder { get; set; }

		void Load();
		void Save();
	}
}