using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Actions
{
	public enum PathType
	{
		FilePath,
		FolderPath,
	}



	[DataContract]
	public class ApplicationPolicy
	{
		public static ApplicationPolicy Create(string applicationPath)
		{
			if (String.IsNullOrWhiteSpace(applicationPath))
			{
				throw new Exception(nameof(applicationPath) + " " + nameof(String.IsNullOrWhiteSpace));
			}

			var fileInfo = new FileInfo(applicationPath);

			if (false == fileInfo.Exists)
			{
				System.Diagnostics.Debug.WriteLine(applicationPath + " is invalid application path.");
				return null;
			}

			if (fileInfo.Extension != ".exe")
			{
				System.Diagnostics.Debug.WriteLine(applicationPath + " is not executable application.");
				return null;
			}

			return new ApplicationPolicy(applicationPath);
		}




		[DataMember]
		public string ApplicationPath { get; private set; }

		[DataMember]
		public TimeSpan MaxProcessTime { get; set; }

		[DataMember]
		public Dictionary<string, string> DefaultParameters { get; private set; }

		[DataMember]
		public List<AppOption> AppParams { get; private set; }

		[DataMember]
		public string KeyPrefix { get; set; } = "-";

		[DataMember]
		public PathType InputPathType { get; set; }
		[DataMember]
		public string InputPathKey { get; set; }

		[DataMember]
		public PathType OutputPathType { get; set; }
		[DataMember]
		public string OutputPathKey { get; set; }


		[DataMember]
		public List<string> PathFilterPartterns { get; private set; }


		private string _AppName;
		public string AppName
		{
			get
			{

				return _AppName
					?? (_AppName = Path.GetFileNameWithoutExtension(ApplicationPath));
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="applicationPath"></param>
		public ApplicationPolicy(string applicationPath)
		{
			ApplicationPath = applicationPath;

			MaxProcessTime = TimeSpan.FromMinutes(1);

			DefaultParameters = new Dictionary<string, string>();
			AppParams = new List<AppOption>();
			PathFilterPartterns = new List<string>();
		}


		public bool IsExistApplicationFile
		{
			get
			{
				if (String.IsNullOrWhiteSpace(ApplicationPath))
				{
					return false;
				}

				return File.Exists(ApplicationPath);
			}
		}


		

		public string MakeArgumentsText(string inputPath, DirectoryInfo outputDir, string name, AppOption param)
		{
			// 入力パスと出力パスの指定
			var inputText = $"-{InputPathKey}";

			// 他のパラメータ
			var otherParamsText = MakeCommandLineParam(param.Arguments);

			var appArguments = $"{inputText} {otherParamsText}";

			return $"{inputText} {otherParamsText}";
		}

		private string MakeCommandLineParam(Dictionary<string, string> parameters)
		{
			StringBuilder sb = new StringBuilder();

			// DefaultParameters < parameters
			foreach (var pair in parameters)
			{
				sb.Append(MakePartOfCommandLineParam(pair));
			}

			foreach (var pair in DefaultParameters)
			{
				if (parameters.ContainsKey(pair.Key))
				{
					continue;
				}

				sb.Append(MakePartOfCommandLineParam(pair));
			}

			return sb.ToString();
		}



		private string MakePartOfCommandLineParam(string key, string value)
		{
			// valueが文字列の場合は""で囲む
			decimal temp;
			if (decimal.TryParse(value, out temp))
			{
				return $"-{key} {value}";
			}
			else
			{
				return $"-{key} \"{value}\"";
			}
		}

		private string MakePartOfCommandLineParam(KeyValuePair<string, string> pair)
		{
			return MakePartOfCommandLineParam(pair.Key, pair.Value);
		}


		public ApplicationExecuteSandbox CreateExecuteSandbox(AppOption param)
		{
			if (false == this.AppParams.Contains(param))
			{
				throw new Exception("invalid AppExecuteParam. it is diffarent ApplicationPolicy param?");
			}

			return new ApplicationExecuteSandbox(this, param);
		}
	}



	public static class ApplicationPolicyHelper
	{
		public static AppOption GetOption(this ApplicationPolicy policy, string optionName)
		{
			return policy.AppParams.SingleOrDefault(x => x.Name == optionName);
		}

	}

}
