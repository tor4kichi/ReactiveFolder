using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.AppPolicy
{
	public class ApplicationExecuteSandbox : BindableBase
	{
		public ApplicationPolicy Policy { get; private set; }
		public AppArgument Param { get; private set; }


		/// <summary>
		/// use ApplicationPolicy.CreateExecuteSandbox()
		/// </summary>
		/// <param name="policy"></param>
		/// <param name="param"></param>
		/// <param name="context"></param>
		internal ApplicationExecuteSandbox(ApplicationPolicy policy, AppArgument param)
		{
			Policy = policy;
			Param = param;
		}

		public bool Validate(ReactiveStreamContext context)
		{
			// TODO: Policy.Validate();
			if (Policy == null)
			{
				return false;
			}

			if (Param == null)
			{
				return false;
			}

			return true;
		}

		public string Execute(ReactiveStreamContext context)
		{
			var argumentText = Policy.MakeArgumentsText(context.SourcePath, context.WorkFolder, Param);

			var processStartInfo = new ProcessStartInfo(Policy.ApplicationPath, argumentText);


			using (var process = Process.Start(processStartInfo))
			{
				if (process.WaitForExit((int)Policy.MaxProcessTime.TotalMilliseconds))
				{

					if (process.ExitCode != 0)
					{
						// エラー終了
						return null;
					}
					else
					{
						// 正常終了
						// TODO: 出力されたファイル名/フォルダ名を捕捉する
						return "";
					}
				}
				else
				{
					// タイムアウトによる終了

					return null;
				}
			}
		}		
	}

}
