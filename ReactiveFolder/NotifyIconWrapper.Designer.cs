namespace ReactiveFolder
{
	partial class NotifyIconWrapper
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region コンポーネント デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotifyIconWrapper));
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItem_Open = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem_Exist = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip1.SuspendLayout();
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
			this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
			this.notifyIcon1.Text = "○個のフォルダをモニタ中";
			this.notifyIcon1.Visible = true;
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_Open,
            this.toolStripMenuItem_Exist});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(140, 48);
			// 
			// toolStripMenuItem_Open
			// 
			this.toolStripMenuItem_Open.Name = "toolStripMenuItem_Open";
			this.toolStripMenuItem_Open.Size = new System.Drawing.Size(139, 22);
			this.toolStripMenuItem_Open.Text = "ウィンドウ表示";
			// 
			// toolStripMenuItem_Exist
			// 
			this.toolStripMenuItem_Exist.Name = "toolStripMenuItem_Exist";
			this.toolStripMenuItem_Exist.Size = new System.Drawing.Size(139, 22);
			this.toolStripMenuItem_Exist.Text = "終了";
			this.contextMenuStrip1.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Open;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Exist;
	}
}
