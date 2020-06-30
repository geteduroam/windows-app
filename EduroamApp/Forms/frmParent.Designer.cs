namespace EduroamApp
{
	partial class frmParent
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.pnlNavigation = new System.Windows.Forms.Panel();
			this.tableLayoutPanelButtons = new System.Windows.Forms.TableLayoutPanel();
			this.btnNext = new System.Windows.Forms.Button();
			this.btnBack = new System.Windows.Forms.Button();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.btnCancel = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.webEduroamLogo = new System.Windows.Forms.WebBrowser();
			this.lblTitle = new System.Windows.Forms.Label();
			this.webLogo = new System.Windows.Forms.WebBrowser();
			this.pbxLogo = new System.Windows.Forms.PictureBox();
			this.pnlContent = new System.Windows.Forms.Panel();
			this.pnlRightMargin = new System.Windows.Forms.Panel();
			this.pnlLeftMargin = new System.Windows.Forms.Panel();
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.pnlLogo = new System.Windows.Forms.Panel();
			this.pnlNavigation.SuspendLayout();
			this.tableLayoutPanelButtons.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbxLogo)).BeginInit();
			this.pnlContent.SuspendLayout();
			this.pnlLogo.SuspendLayout();
			this.SuspendLayout();
			//
			// pnlNavigation
			//
			this.pnlNavigation.BackColor = System.Drawing.Color.White;
			this.pnlNavigation.Controls.Add(this.tableLayoutPanelButtons);
			this.pnlNavigation.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlNavigation.Location = new System.Drawing.Point(0, 501);
			this.pnlNavigation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pnlNavigation.Name = "pnlNavigation";
			this.pnlNavigation.Size = new System.Drawing.Size(529, 83);
			this.pnlNavigation.TabIndex = 0;
			this.pnlNavigation.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlNavigation_Paint);
			//
			// tableLayoutPanelButtons
			//
			this.tableLayoutPanelButtons.ColumnCount = 3;
			this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33224F));
			this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33388F));
			this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33388F));
			this.tableLayoutPanelButtons.Controls.Add(this.btnNext, 1, 0);
			this.tableLayoutPanelButtons.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelButtons.Name = "tableLayoutPanelButtons";
			this.tableLayoutPanelButtons.RowCount = 1;
			this.tableLayoutPanelButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelButtons.Size = new System.Drawing.Size(526, 77);
			this.tableLayoutPanelButtons.TabIndex = 0;
			//
			// btnNext
			//
			this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.btnNext.BackColor = System.Drawing.SystemColors.Highlight;
			this.btnNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnNext.ForeColor = System.Drawing.SystemColors.ControlLight;
			this.btnNext.Location = new System.Drawing.Point(179, 5);
			this.btnNext.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnNext.Name = "btnNext";
			this.btnNext.Size = new System.Drawing.Size(167, 67);
			this.btnNext.TabIndex = 0;
			this.btnNext.Text = "Next >";
			this.btnNext.UseVisualStyleBackColor = false;
			this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
			//
			// btnBack
			//
			this.btnBack.BackColor = System.Drawing.Color.White;
			this.btnBack.Location = new System.Drawing.Point(6, 9);
			this.btnBack.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(80, 37);
			this.btnBack.TabIndex = 0;
			this.btnBack.Text = "< Back";
			this.btnBack.UseVisualStyleBackColor = false;
			this.btnBack.Visible = false;
			this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.tableLayoutPanel1.Controls.Add(this.btnCancel, 2, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(126, 249);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(348, 100);
			this.tableLayoutPanel1.TabIndex = 0;
			//
			// btnCancel
			//
			this.btnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnCancel.Location = new System.Drawing.Point(236, 5);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(108, 90);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			//
			// panel1
			//
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Controls.Add(this.lblTitle);
			this.panel1.Controls.Add(this.pnlLogo);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(529, 116);
			this.panel1.TabIndex = 4;
			//
			// webEduroamLogo
			//
			this.webEduroamLogo.AllowWebBrowserDrop = false;
			this.webEduroamLogo.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.webEduroamLogo.IsWebBrowserContextMenuEnabled = false;
			this.webEduroamLogo.Location = new System.Drawing.Point(164, 10);
			this.webEduroamLogo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.webEduroamLogo.MinimumSize = new System.Drawing.Size(30, 31);
			this.webEduroamLogo.Name = "webEduroamLogo";
			this.webEduroamLogo.ScriptErrorsSuppressed = true;
			this.webEduroamLogo.ScrollBarsEnabled = false;
			this.webEduroamLogo.Size = new System.Drawing.Size(173, 66);
			this.webEduroamLogo.TabIndex = 11;
			this.webEduroamLogo.WebBrowserShortcutsEnabled = false;
			//
			// lblTitle
			//
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(13, 82);
			this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(503, 29);
			this.lblTitle.TabIndex = 7;
			this.lblTitle.Text = "Connection status";
			this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			// webLogo
			//
			this.webLogo.AllowWebBrowserDrop = false;
			this.webLogo.IsWebBrowserContextMenuEnabled = false;
			this.webLogo.Location = new System.Drawing.Point(170, 10);
			this.webLogo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.webLogo.MinimumSize = new System.Drawing.Size(30, 31);
			this.webLogo.Name = "webLogo";
			this.webLogo.ScriptErrorsSuppressed = true;
			this.webLogo.ScrollBarsEnabled = false;
			this.webLogo.Size = new System.Drawing.Size(152, 66);
			this.webLogo.TabIndex = 10;
			this.webLogo.Url = new System.Uri("", System.UriKind.Relative);
			this.webLogo.Visible = false;
			this.webLogo.WebBrowserShortcutsEnabled = false;
			//
			// pbxLogo
			//
			this.pbxLogo.Location = new System.Drawing.Point(155, 9);
			this.pbxLogo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pbxLogo.Name = "pbxLogo";
			this.pbxLogo.Size = new System.Drawing.Size(195, 67);
			this.pbxLogo.TabIndex = 9;
			this.pbxLogo.TabStop = false;
			this.pbxLogo.Visible = false;
			//
			// pnlContent
			//
			this.pnlContent.AutoSize = true;
			this.pnlContent.BackColor = System.Drawing.Color.White;
			this.pnlContent.Controls.Add(this.tableLayoutPanel1);
			this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlContent.Location = new System.Drawing.Point(10, 116);
			this.pnlContent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pnlContent.Name = "pnlContent";
			this.pnlContent.Size = new System.Drawing.Size(506, 385);
			this.pnlContent.TabIndex = 7;
			//
			// pnlRightMargin
			//
			this.pnlRightMargin.BackColor = System.Drawing.Color.White;
			this.pnlRightMargin.Dock = System.Windows.Forms.DockStyle.Right;
			this.pnlRightMargin.Location = new System.Drawing.Point(516, 116);
			this.pnlRightMargin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pnlRightMargin.Name = "pnlRightMargin";
			this.pnlRightMargin.Size = new System.Drawing.Size(13, 385);
			this.pnlRightMargin.TabIndex = 6;
			//
			// pnlLeftMargin
			//
			this.pnlLeftMargin.BackColor = System.Drawing.Color.White;
			this.pnlLeftMargin.Dock = System.Windows.Forms.DockStyle.Left;
			this.pnlLeftMargin.Location = new System.Drawing.Point(0, 116);
			this.pnlLeftMargin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pnlLeftMargin.Name = "pnlLeftMargin";
			this.pnlLeftMargin.Size = new System.Drawing.Size(10, 385);
			this.pnlLeftMargin.TabIndex = 5;
			//
			// notifyIcon
			//
			this.notifyIcon.Icon = global::EduroamApp.Properties.Resources.geteduroam;
			this.notifyIcon.Text = "geteduroam";
			this.notifyIcon.Visible = true;
			//
			// pnlLogo
			//
			this.pnlLogo.Controls.Add(this.btnBack);
			this.pnlLogo.Controls.Add(this.pbxLogo);
			this.pnlLogo.Controls.Add(this.webEduroamLogo);
			this.pnlLogo.Controls.Add(this.webLogo);
			this.pnlLogo.Location = new System.Drawing.Point(12, 3);
			this.pnlLogo.Name = "pnlLogo";
			this.pnlLogo.Size = new System.Drawing.Size(504, 76);
			this.pnlLogo.TabIndex = 12;
			//
			// frmParent
			//
			this.AcceptButton = this.btnNext;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(529, 584);
			this.Controls.Add(this.pnlContent);
			this.Controls.Add(this.pnlRightMargin);
			this.Controls.Add(this.pnlLeftMargin);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.pnlNavigation);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.Name = "frmParent";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "geteduroam";
			this.Load += new System.EventHandler(this.frmParent_Load);
			this.pnlNavigation.ResumeLayout(false);
			this.tableLayoutPanelButtons.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbxLogo)).EndInit();
			this.pnlContent.ResumeLayout(false);
			this.pnlLogo.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel pnlNavigation;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button btnBack;
		private System.Windows.Forms.Button btnNext;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Panel pnlContent;
		private System.Windows.Forms.PictureBox pbxLogo;
		private System.Windows.Forms.WebBrowser webLogo;
		private System.Windows.Forms.WebBrowser webEduroamLogo;
		private System.Windows.Forms.Panel pnlRightMargin;
		private System.Windows.Forms.Panel pnlLeftMargin;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelButtons;
		private System.Windows.Forms.NotifyIcon notifyIcon;
		private System.Windows.Forms.Panel pnlLogo;
	}
}