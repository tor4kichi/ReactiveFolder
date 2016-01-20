using ReactiveFolder.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ReactiveFolder
{
	public partial class NotifyIconWrapper : Component
	{
		public NotifyIconWrapper()
		{
			InitializeComponent();

			this.toolStripMenuItem_Open.Click += ToolStripMenuItem_Open_Click;
			this.toolStripMenuItem_Exist.Click += ToolStripMenuItem_Exist_Click;
		}

		public NotifyIconWrapper(IContainer container)
		{
			container.Add(this);

			InitializeComponent();
		}

		private void ToolStripMenuItem_Exist_Click(object sender, EventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void ToolStripMenuItem_Open_Click(object sender, EventArgs e)
		{
			// MainWindow を生成、表示
			App.Current.MainWindow.Show();
		}

		
	}
}
