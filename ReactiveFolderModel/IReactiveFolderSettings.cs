namespace ReactiveFolder.Models
{
	public interface IReactiveFolderSettings
	{
		string SaveFolder { get; set; }
		int DefaultMonitorIntervalSeconds { get; set; }

		void Load();
		void Save();
	}
}