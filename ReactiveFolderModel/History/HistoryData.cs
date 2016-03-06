using ReactiveFolder.Models.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.History
{
	// 一回のアクションごとに処理されたファイルの履歴
	/*
		使用したAppLaunchReactiveAction
		Array＜HistoryDataByFile＞
		処理元のリアクションまたはインスタントアクションのファイルパス
	*/

	public class HistoryData
	{
		public AppLaunchReactiveAction[] Actions { get; set; }
		public HistoryDataByFile[] FileHistories { get; set; }
	
		/// <summary>
		/// インスタントアクションやリアクションなどの実行元のファイルパス
		/// </summary>
		public string ActionSourceFilePath { get; set; }
	}
}
