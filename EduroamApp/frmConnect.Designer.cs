namespace EduroamApp
{
	partial class frmConnect
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
			this.lblStatus = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.pboStatus = new System.Windows.Forms.PictureBox();
			this.lblConnectFailed = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pboStatus)).BeginInit();
			this.SuspendLayout();
			//
			// lblStatus
			//
			this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblStatus.AutoSize = true;
			this.lblStatus.Location = new System.Drawing.Point(3, 16);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(70, 13);
			this.lblStatus.TabIndex = 7;
			this.lblStatus.Text = "Connecting...";
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 171F));
			this.tableLayoutPanel1.Controls.Add(this.lblStatus, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.pboStatus, 1, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, -2);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(247, 45);
			this.tableLayoutPanel1.TabIndex = 9;
			//
			// pboStatus
			//
			this.pboStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.pboStatus.Image = global::EduroamApp.Properties.Resources.ajax_loader;
			this.pboStatus.Location = new System.Drawing.Point(79, 14);
			this.pboStatus.Name = "pboStatus";
			this.pboStatus.Size = new System.Drawing.Size(16, 16);
			this.pboStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pboStatus.TabIndex = 8;
			this.pboStatus.TabStop = false;
			//
			// lblConnectFailed
			//
			this.lblConnectFailed.AutoSize = true;
			this.lblConnectFailed.Location = new System.Drawing.Point(3, 46);
			this.lblConnectFailed.Name = "lblConnectFailed";
			this.lblConnectFailed.Size = new System.Drawing.Size(266, 52);
			this.lblConnectFailed.TabIndex = 10;
			this.lblConnectFailed.Text = "Press Back if you want to choose a different config file.\r\n\r\nAlternatively, you c" +
	"an try to log in with your username \r\nand password by pressing Next.";
			this.lblConnectFailed.Visible = false;
			//
			// frmConnect
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(378, 246);
			this.Controls.Add(this.lblConnectFailed);
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "frmConnect";
			this.Text = "frm5";
			this.Load += new System.EventHandler(this.frm5_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pboStatus)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.PictureBox pboStatus;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label lblConnectFailed;
	}
}