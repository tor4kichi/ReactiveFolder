using ReactiveFolder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.History
{
	/*
		入力ファイルパス
		出力ファイルパス
		開始時刻
		終了時刻
		処理結果
	*/

	public class HistoryDataByFile
	{
		public string InputFilePath { get; set; }

		/// <summary>
		/// ※ IsSuccessedがtrueであってもすでに出力済みの場合にはOutputFilePathがnullの場合がある
		/// </summary>
		public string OutputFilePath { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool IsSuccessed { get; set; }
	}
}
