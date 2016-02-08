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

		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// クローズ処理をキャンセルして、タスクバーの表示も消す
			e.Cancel = true;
			this.WindowState = System.Windows.WindowState.Minimized;
			this.ShowInTaskbar = false;
		}



	}
}
