using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.AppPolicy
{
	public class ApplicationExecuteSandbox : BindableBase
	{
		public ApplicationPolicy Policy { get; private set; }
		public AppOptionInstance[] Options { get; private set; }

		/// <summary>
		/// use ApplicationPolicy.CreateExecuteSandbox()
		/// </summary>
		/// <param name="policy"></param>
		/// <param name="param"></param>
		/// <param name="context"></param>
		internal ApplicationExecuteSandbox(ApplicationPolicy policy, AppOptionInstance[] options)
		{
			Policy = policy;
			Options = options;
		}

		public bool Validate(ReactiveStreamContext context)
		{
			if (Policy == null)
			{
				return false;
			}

			if (false == Policy.IsExistApplicationFile)
			{
				return false;
			}

			// TODO: Optionsのチェック

			return true;
		}

		private bool ValidateExecuteResult(string sourctPath, DirectoryInfo destFolder)
		{
			// TODO: 

			return true;
		}

		public bool Execute(string sourctPath, DirectoryInfo destFolder)
		{
			var argumentText = Policy.MakeCommandLineOptionText(sourctPath, destFolder, Options);

			var processStartInfo = new ProcessStartInfo(Policy.ApplicationPath, argumentText);

			processStartInfo.WorkingDirectory = destFolder.FullName;

			processStartInfo.CreateNoWindow = true;
			processStartInfo.UseShellExecute = false;

#if DEBUG
			processStartInfo.RedirectStandardInput = false;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.RedirectStandardError = true;
#endif

			using (var process = Process.Start(processStartInfo))
			{
#if DEBUG
				process.ErrorDataReceived += Process_ErrorDataReceived;
				process.OutputDataReceived += Process_OutputDataReceived;

				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
#endif

				if (process.WaitForExit((int)Policy.MaxProcessTime.TotalMilliseconds))
				{
					if (process.ExitCode != 0)
					{
						return false;
					}
				}
				else
				{
					// タイムアウトによる終了

					return false;
				}
			}


			return ValidateExecuteResult(sourctPath, destFolder);
		}
#if DEBUG
		private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{

			Console.WriteLine(e.Data);

		}

		private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{

			Console.WriteLine(e.Data);
		}
#endif


		
	}

}
