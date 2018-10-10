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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmParent));
			this.pnlNavigation = new System.Windows.Forms.Panel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnBack = new System.Windows.Forms.Button();
			this.btnNext = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.pnlNavBot = new System.Windows.Forms.Panel();
			this.pnlNavTop = new System.Windows.Forms.Panel();
			this.pnlLeft = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.pnlLogoBot = new System.Windows.Forms.Panel();
			this.pnlLogoTop = new System.Windows.Forms.Panel();
			this.pnlLogoLeft = new System.Windows.Forms.Panel();
			this.pnlLogoRight = new System.Windows.Forms.Panel();
			this.webLogo = new System.Windows.Forms.WebBrowser();
			this.pbxLogo = new System.Windows.Forms.PictureBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.lblTitle = new System.Windows.Forms.Label();
			this.pnlLeftMargin = new System.Windows.Forms.Panel();
			this.pnlRightMargin = new System.Windows.Forms.Panel();
			this.pnlContent = new System.Windows.Forms.Panel();
			this.webEduroamLogo = new System.Windows.Forms.WebBrowser();
			this.pnlNavigation.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.pnlLeft.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbxLogo)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			//
			// pnlNavigation
			//
			this.pnlNavigation.Controls.Add(this.tableLayoutPanel1);
			this.pnlNavigation.Controls.Add(this.panel2);
			this.pnlNavigation.Controls.Add(this.pnlNavBot);
			this.pnlNavigation.Controls.Add(this.pnlNavTop);
			this.pnlNavigation.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlNavigation.Location = new System.Drawing.Point(0, 366);
			this.pnlNavigation.Name = "pnlNavigation";
			this.pnlNavigation.Size = new System.Drawing.Size(608, 42);
			this.pnlNavigation.TabIndex = 0;
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.tableLayoutPanel1.Controls.Add(this.btnCancel, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.btnBack, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.btnNext, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(370, 6);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(232, 30);
			this.tableLayoutPanel1.TabIndex = 0;
			//
			// btnCancel
			//
			this.btnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnCancel.Location = new System.Drawing.Point(157, 3);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(72, 24);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			//
			// btnBack
			//
			this.btnBack.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnBack.Location = new System.Drawing.Point(3, 3);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(71, 24);
			this.btnBack.TabIndex = 0;
			this.btnBack.Text = "< Back";
			this.btnBack.UseVisualStyleBackColor = true;
			this.btnBack.Visible = false;
			this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
			//
			// btnNext
			//
			this.btnNext.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnNext.Location = new System.Drawing.Point(80, 3);
			this.btnNext.Name = "btnNext";
			this.btnNext.Size = new System.Drawing.Size(71, 24);
			this.btnNext.TabIndex = 0;
			this.btnNext.Text = "Next >";
			this.btnNext.UseVisualStyleBackColor = true;
			this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
			//
			// panel2
			//
			this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel2.Location = new System.Drawing.Point(602, 6);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(6, 30);
			this.panel2.TabIndex = 4;
			//
			// pnlNavBot
			//
			this.pnlNavBot.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlNavBot.Location = new System.Drawing.Point(0, 36);
			this.pnlNavBot.Name = "pnlNavBot";
			this.pnlNavBot.Size = new System.Drawing.Size(608, 6);
			this.pnlNavBot.TabIndex = 2;
			//
			// pnlNavTop
			//
			this.pnlNavTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlNavTop.Location = new System.Drawing.Point(0, 0);
			this.pnlNavTop.Name = "pnlNavTop";
			this.pnlNavTop.Size = new System.Drawing.Size(608, 6);
			this.pnlNavTop.TabIndex = 1;
			this.pnlNavTop.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlNavTop_Paint);
			//
			// pnlLeft
			//
			this.pnlLeft.BackColor = System.Drawing.Color.White;
			this.pnlLeft.Controls.Add(this.webEduroamLogo);
			this.pnlLeft.Controls.Add(this.panel3);
			this.pnlLeft.Controls.Add(this.pnlLogoBot);
			this.pnlLeft.Controls.Add(this.pnlLogoTop);
			this.pnlLeft.Controls.Add(this.pnlLogoLeft);
			this.pnlLeft.Controls.Add(this.pnlLogoRight);
			this.pnlLeft.Controls.Add(this.webLogo);
			this.pnlLeft.Controls.Add(this.pbxLogo);
			this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
			this.pnlLeft.Location = new System.Drawing.Point(0, 0);
			this.pnlLeft.Name = "pnlLeft";
			this.pnlLeft.Size = new System.Drawing.Size(160, 366);
			this.pnlLeft.TabIndex = 2;
			//
			// panel3
			//
			this.panel3.Location = new System.Drawing.Point(8, 92);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(144, 28);
			this.panel3.TabIndex = 8;
			//
			// pnlLogoBot
			//
			this.pnlLogoBot.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlLogoBot.Location = new System.Drawing.Point(8, 300);
			this.pnlLogoBot.Name = "pnlLogoBot";
			this.pnlLogoBot.Size = new System.Drawing.Size(144, 66);
			this.pnlLogoBot.TabIndex = 6;
			//
			// pnlLogoTop
			//
			this.pnlLogoTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlLogoTop.Location = new System.Drawing.Point(8, 0);
			this.pnlLogoTop.Name = "pnlLogoTop";
			this.pnlLogoTop.Size = new System.Drawing.Size(144, 27);
			this.pnlLogoTop.TabIndex = 5;
			//
			// pnlLogoLeft
			//
			this.pnlLogoLeft.Dock = System.Windows.Forms.DockStyle.Left;
			this.pnlLogoLeft.Location = new System.Drawing.Point(0, 0);
			this.pnlLogoLeft.Name = "pnlLogoLeft";
			this.pnlLogoLeft.Size = new System.Drawing.Size(8, 366);
			this.pnlLogoLeft.TabIndex = 3;
			//
			// pnlLogoRight
			//
			this.pnlLogoRight.Dock = System.Windows.Forms.DockStyle.Right;
			this.pnlLogoRight.Location = new System.Drawing.Point(152, 0);
			this.pnlLogoRight.Name = "pnlLogoRight";
			this.pnlLogoRight.Size = new System.Drawing.Size(8, 366);
			this.pnlLogoRight.TabIndex = 2;
			this.pnlLogoRight.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlLogoRight_Paint);
			//
			// webLogo
			//
			this.webLogo.AllowWebBrowserDrop = false;
			this.webLogo.IsWebBrowserContextMenuEnabled = false;
			this.webLogo.Location = new System.Drawing.Point(8, 120);
			this.webLogo.MinimumSize = new System.Drawing.Size(20, 20);
			this.webLogo.Name = "webLogo";
			this.webLogo.ScriptErrorsSuppressed = true;
			this.webLogo.ScrollBarsEnabled = false;
			this.webLogo.Size = new System.Drawing.Size(144, 115);
			this.webLogo.TabIndex = 10;
			this.webLogo.Url = new System.Uri("", System.UriKind.Relative);
			this.webLogo.Visible = false;
			this.webLogo.WebBrowserShortcutsEnabled = false;
			//
			// pbxLogo
			//
			this.pbxLogo.Location = new System.Drawing.Point(8, 120);
			this.pbxLogo.Name = "pbxLogo";
			this.pbxLogo.Size = new System.Drawing.Size(144, 115);
			this.pbxLogo.TabIndex = 9;
			this.pbxLogo.TabStop = false;
			this.pbxLogo.Visible = false;
			//
			// panel1
			//
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Controls.Add(this.lblTitle);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(160, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(448, 120);
			this.panel1.TabIndex = 4;
			//
			// lblTitle
			//
			this.lblTitle.AutoSize = true;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(35, 59);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(138, 20);
			this.lblTitle.TabIndex = 7;
			this.lblTitle.Text = "Connection status";
			//
			// pnlLeftMargin
			//
			this.pnlLeftMargin.BackColor = System.Drawing.Color.White;
			this.pnlLeftMargin.Dock = System.Windows.Forms.DockStyle.Left;
			this.pnlLeftMargin.Location = new System.Drawing.Point(160, 120);
			this.pnlLeftMargin.Name = "pnlLeftMargin";
			this.pnlLeftMargin.Size = new System.Drawing.Size(35, 246);
			this.pnlLeftMargin.TabIndex = 5;
			//
			// pnlRightMargin
			//
			this.pnlRightMargin.BackColor = System.Drawing.Color.White;
			this.pnlRightMargin.Dock = System.Windows.Forms.DockStyle.Right;
			this.pnlRightMargin.Location = new System.Drawing.Point(573, 120);
			this.pnlRightMargin.Name = "pnlRightMargin";
			this.pnlRightMargin.Size = new System.Drawing.Size(35, 246);
			this.pnlRightMargin.TabIndex = 6;
			//
			// pnlContent
			//
			this.pnlContent.AutoSize = true;
			this.pnlContent.BackColor = System.Drawing.Color.White;
			this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlContent.Location = new System.Drawing.Point(195, 120);
			this.pnlContent.Name = "pnlContent";
			this.pnlContent.Size = new System.Drawing.Size(378, 246);
			this.pnlContent.TabIndex = 7;
			//
			// webEduroamLogo
			//
			this.webEduroamLogo.AllowWebBrowserDrop = false;
			this.webEduroamLogo.Dock = System.Windows.Forms.DockStyle.Top;
			this.webEduroamLogo.IsWebBrowserContextMenuEnabled = false;
			this.webEduroamLogo.Location = new System.Drawing.Point(8, 27);
			this.webEduroamLogo.MinimumSize = new System.Drawing.Size(20, 20);
			this.webEduroamLogo.Name = "webEduroamLogo";
			this.webEduroamLogo.ScriptErrorsSuppressed = true;
			this.webEduroamLogo.ScrollBarsEnabled = false;
			this.webEduroamLogo.Size = new System.Drawing.Size(144, 65);
			this.webEduroamLogo.TabIndex = 11;
			this.webEduroamLogo.WebBrowserShortcutsEnabled = false;
			//
			// frmParent
			//
			this.AcceptButton = this.btnNext;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(608, 408);
			this.Controls.Add(this.pnlContent);
			this.Controls.Add(this.pnlRightMargin);
			this.Controls.Add(this.pnlLeftMargin);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.pnlLeft);
			this.Controls.Add(this.pnlNavigation);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmParent";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "GetEduroam";
			this.Load += new System.EventHandler(this.frmParent_Load);
			this.pnlNavigation.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.pnlLeft.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbxLogo)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel pnlNavigation;
		private System.Windows.Forms.Panel pnlLeft;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button btnBack;
		private System.Windows.Forms.Button btnNext;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Panel pnlLeftMargin;
		private System.Windows.Forms.Panel pnlRightMargin;
		private System.Windows.Forms.Panel pnlContent;
		private System.Windows.Forms.Panel pnlNavTop;
		private System.Windows.Forms.Panel pnlNavBot;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel pnlLogoLeft;
		private System.Windows.Forms.Panel pnlLogoRight;
		private System.Windows.Forms.Panel pnlLogoBot;
		private System.Windows.Forms.Panel pnlLogoTop;
		private System.Windows.Forms.PictureBox pbxLogo;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.WebBrowser webLogo;
		private System.Windows.Forms.WebBrowser webEduroamLogo;
	}
}