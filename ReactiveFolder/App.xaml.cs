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
using ReactiveFolder.Models;
using Prism.Events;

namespace ReactiveFolder
{
	// 参考: タスクトレイ常駐アプリの作成
	// http://garafu.blogspot.jp/2015/06/dev-tasktray-residentapplication.html


	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		/// <summary>
		/// System.Windows.Application.Startup イベント を発生させます。
		/// </summary>
		/// <param name="e"></param>
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// タスクトレイ常駐アプリとして振る舞うため
			// ウィンドウを閉じる動作でアプリを終了しないようにする
			this.ShutdownMode = ShutdownMode.OnExplicitShutdown;


			// Prismのアプリ立ち上げ作法に則る
			var bs = new Bootstrapper(e.Args);

			bs.Run();

		}

		/// <summary>
		/// System.Windows.Application.Exit イベント を発生させます。
		/// </summary>
		/// <param name="e"></param>
		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
		}
	}
}
