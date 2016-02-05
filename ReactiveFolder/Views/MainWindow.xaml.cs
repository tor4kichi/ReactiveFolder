using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReactiveFolder.Views
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			Closing += Window_Closing;

			IsOpenSideMenu = false;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// クローズ処理をキャンセルして、タスクバーの表示も消す
			e.Cancel = true;
			this.WindowState = System.Windows.WindowState.Minimized;
			this.ShowInTaskbar = false;
		}



		public static readonly DependencyProperty IsOpenSideMenuProperty =
			DependencyProperty.Register("IsOpenSideMenu",
										typeof(bool),
										typeof(MainWindow),
										new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsOpenSideMenuChanged)));

		public bool IsOpenSideMenu
		{
			get { return (bool)GetValue(IsOpenSideMenuProperty); }
			set { SetValue(IsOpenSideMenuProperty, value); }
		}


		// 3. 依存プロパティが変更されたとき呼ばれるコールバック関数の定義
		private static void OnIsOpenSideMenuChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			// オブジェクトを取得して処理する
			MainWindow window = obj as MainWindow;

			if (window.IsOpenSideMenu)
			{
				window.SideMenuContainer.Visibility = Visibility.Visible;
			}
			else
			{
				window.SideMenuContainer.Visibility = Visibility.Collapsed;
			}


		}

	}
}
