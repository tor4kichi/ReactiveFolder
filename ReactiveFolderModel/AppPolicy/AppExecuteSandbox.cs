using Microsoft.Practices.Prism.Mvvm;
using ReactiveFolder.Models.Util;
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
		public ApplicationPolicy AppPolicy { get; private set; }
		public AppOptionInstance[] Options { get; private set; }

		public string ResultPath { get; private set; }
		public FolderItemType OutputPathType { get; private set; }

		/// <summary>
		/// use ApplicationPolicy.CreateExecuteSandbox()
		/// </summary>
		/// <param name="policy"></param>
		/// <param name="param"></param>
		/// <param name="context"></param>
		internal ApplicationExecuteSandbox(ApplicationPolicy policy, AppOptionInstance[] options)
		{
			AppPolicy = policy;
			Options = options;
		}

		public bool Validate(ReactiveStreamContext context)
		{
			if (AppPolicy == null)
			{
				return false;
			}

			if (false == AppPolicy.IsExistApplicationFile)
			{
				return false;
			}

			// TODO: Optionsのチェック

			return true;
		}

		private bool ValidateExecuteResult()
		{
			if (OutputPathType == FolderItemType.File)
			{
				return File.Exists(ResultPath);
			}
			else if (OutputPathType == FolderItemType.Folder)
			{
				return Directory.Exists(ResultPath);
			}
			else
			{
				return false;
			}
		}


		

		public bool Execute(string inputPath, DirectoryInfo destFolder)
		{
			var argumentText = MakeArgumentText(inputPath, destFolder);

			var processStartInfo = new ProcessStartInfo(AppPolicy.ApplicationPath, argumentText);

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

				if (process.WaitForExit((int)AppPolicy.MaxProcessTime.TotalMilliseconds))
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


			return ValidateExecuteResult();


		}

		private string MakeArgumentText(string inputPath, DirectoryInfo destFolder)
		{
			// Note: コマンドライン引数として使用するオプションを一つのリストにまとめて
			// オプションからコマンドライン引数の文字列を生成します。
			var finalOptions = Options.ToList();

			// input
			var inputOptionInstance = AppPolicy.InputOption.CreateInstance();
			inputOptionInstance.Values[0].Value = inputPath;
			finalOptions.Add(inputOptionInstance);


			// output
			var outputValueInstance = finalOptions.FindOutputOptionInstance();

			if (outputValueInstance == null)
			{
				outputValueInstance = AppPolicy.SameInputOutputOption.CreateInstance();
				finalOptions.Add(outputValueInstance);
			}

			var outputPath = Path.Combine(destFolder.FullName, Path.GetFileName(inputPath));
			outputValueInstance.Values[0].Value = outputPath;


			var decl = outputValueInstance.OptionDeclaration as AppOutputOptionDeclaration;			
			ResultPath = (string)decl.OutputPathProperty.ConvertOptionText(outputPath);

			OutputPathType = (decl as AppOutputOptionDeclaration).OutputType;


			var argumentText = AppPolicy.AppOptionsToArgumentText(finalOptions);

			return argumentText;
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
