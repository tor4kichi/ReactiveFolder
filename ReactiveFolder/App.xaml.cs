using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Mvvm;
using System.Reflection;
using System.Globalization;
using System.Threading;
using ReactiveFolder;
using ReactiveFolder.Model;

namespace ReactiveFolder
{
	// 参考: タスクトレイ常駐アプリの作成
	// http://garafu.blogspot.jp/2015/06/dev-tasktray-residentapplication.html


	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		private const string ViewNamespace = "Views";
		private const string ViewModelNamespace = "ViewModels";


		/// <summary>
		/// タスクトレイに表示するアイコン
		/// </summary>
		private NotifyIconWrapper _NotifyIcon;

		//		private FolderReactionMonitorModel _MonitorModel;

		/// <summary>
		/// System.Windows.Application.Startup イベント を発生させます。
		/// </summary>
		/// <param name="e"></param>
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
			this._NotifyIcon = new NotifyIconWrapper();

			Bootstrapper bs = new Bootstrapper();
			bs.Run();

			
			// Model
			//			_MonitorModel = 
			//			_Container


			// 監視処理を開始する
#if !DEBUG
			//			_MonitorModel.Start(TimeSpan.FromSeconds(600));
#else
//			var subject = _MonitorModel.Debug_Start();

//			subject.OnNext(0);
//			subject.OnCompleted();

//			subject.Dispose();

#endif

		}

		/// <summary>
		/// System.Windows.Application.Exit イベント を発生させます。
		/// </summary>
		/// <param name="e"></param>
		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);

			
			this._NotifyIcon.Dispose();
		}
	}
}
