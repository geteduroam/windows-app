namespace EduroamApp
{
	partial class frmLogin
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
			this.label1 = new System.Windows.Forms.Label();
			this.txtUsername = new System.Windows.Forms.TextBox();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.lblInst = new System.Windows.Forms.Label();
			this.lblRules = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.lblStatus = new System.Windows.Forms.Label();
			this.pnlEduNotAvail = new System.Windows.Forms.Panel();
			this.lblConnectFailed = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.lblEdu2 = new System.Windows.Forms.Label();
			this.lblEduNotAvail = new System.Windows.Forms.Label();
			this.pbxStatus = new System.Windows.Forms.PictureBox();
			this.panel1.SuspendLayout();
			this.pnlEduNotAvail.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbxStatus)).BeginInit();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(136, 9);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(260, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "Enter your username and password";
			//
			// txtUsername
			//
			this.txtUsername.ForeColor = System.Drawing.SystemColors.GrayText;
			this.txtUsername.Location = new System.Drawing.Point(156, 44);
			this.txtUsername.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtUsername.Name = "txtUsername";
			this.txtUsername.Size = new System.Drawing.Size(197, 26);
			this.txtUsername.TabIndex = 1;
			this.txtUsername.Text = "Username";
			this.txtUsername.TextChanged += new System.EventHandler(this.txtUsername_TextChanged);
			this.txtUsername.Enter += new System.EventHandler(this.txtUsername_Enter);
			this.txtUsername.Leave += new System.EventHandler(this.txtUsername_Leave);
			//
			// txtPassword
			//
			this.txtPassword.ForeColor = System.Drawing.SystemColors.GrayText;
			this.txtPassword.Location = new System.Drawing.Point(156, 74);
			this.txtPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.Size = new System.Drawing.Size(197, 26);
			this.txtPassword.TabIndex = 2;
			this.txtPassword.Text = "Password";
			this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
			this.txtPassword.Enter += new System.EventHandler(this.txtPassword_Enter);
			this.txtPassword.Leave += new System.EventHandler(this.txtPassword_Leave);
			//
			// lblInst
			//
			this.lblInst.AutoSize = true;
			this.lblInst.Location = new System.Drawing.Point(353, 47);
			this.lblInst.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblInst.Name = "lblInst";
			this.lblInst.Size = new System.Drawing.Size(127, 20);
			this.lblInst.TabIndex = 0;
			this.lblInst.Text = "@institution.com";
			this.lblInst.Visible = false;
			//
			// lblRules
			//
			this.lblRules.Location = new System.Drawing.Point(63, 105);
			this.lblRules.Name = "lblRules";
			this.lblRules.Size = new System.Drawing.Size(393, 112);
			this.lblRules.TabIndex = 3;
			this.lblRules.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			// panel1
			//
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.lblStatus);
			this.panel1.Controls.Add(this.pbxStatus);
			this.panel1.Controls.Add(this.pnlEduNotAvail);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.lblRules);
			this.panel1.Controls.Add(this.txtUsername);
			this.panel1.Controls.Add(this.lblInst);
			this.panel1.Controls.Add(this.txtPassword);
			this.panel1.Location = new System.Drawing.Point(-1, 35);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(512, 281);
			this.panel1.TabIndex = 4;
			//
			// lblStatus
			//
			this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblStatus.Location = new System.Drawing.Point(18, 204);
			this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(480, 42);
			this.lblStatus.TabIndex = 8;
			this.lblStatus.Text = "Connecting...";
			this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblStatus.Visible = false;
			this.lblStatus.Click += new System.EventHandler(this.lblStatus_Click);
			//
			// pnlEduNotAvail
			//
			this.pnlEduNotAvail.Controls.Add(this.lblConnectFailed);
			this.pnlEduNotAvail.Controls.Add(this.label2);
			this.pnlEduNotAvail.Controls.Add(this.label3);
			this.pnlEduNotAvail.Controls.Add(this.label4);
			this.pnlEduNotAvail.Controls.Add(this.label5);
			this.pnlEduNotAvail.Controls.Add(this.lblEdu2);
			this.pnlEduNotAvail.Controls.Add(this.lblEduNotAvail);
			this.pnlEduNotAvail.Location = new System.Drawing.Point(23, 271);
			this.pnlEduNotAvail.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pnlEduNotAvail.Name = "pnlEduNotAvail";
			this.pnlEduNotAvail.Size = new System.Drawing.Size(457, 206);
			this.pnlEduNotAvail.TabIndex = 18;
			this.pnlEduNotAvail.Visible = false;
			//
			// lblConnectFailed
			//
			this.lblConnectFailed.Location = new System.Drawing.Point(18, 1);
			this.lblConnectFailed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblConnectFailed.Name = "lblConnectFailed";
			this.lblConnectFailed.Size = new System.Drawing.Size(415, 78);
			this.lblConnectFailed.TabIndex = 11;
			this.lblConnectFailed.Text = "Press Back if you want to try again, connect to a different institution or select" +
	" a different config file.";
			this.lblConnectFailed.Visible = false;
			//
			// label2
			//
			this.label2.Location = new System.Drawing.Point(87, 125);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(312, 52);
			this.label2.TabIndex = 21;
			this.label2.Text = "Delete your configuration and exit the application.\r\n";
			//
			// label3
			//
			this.label3.Location = new System.Drawing.Point(113, 69);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(415, 78);
			this.label3.TabIndex = 10;
			this.label3.Text = "Press Back if you want to try again, connect to a different institution or select" +
	" a different config file.";
			this.label3.Visible = false;
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(3, 157);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(86, 20);
			this.label4.TabIndex = 20;
			this.label4.Text = "Cancel - ";
			//
			// label5
			//
			this.label5.Location = new System.Drawing.Point(87, 80);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(370, 78);
			this.label5.TabIndex = 19;
			this.label5.Text = "Save your current configuration.\r\nYou will be able to automatically connect to ed" +
	"uroam when it becomes available.";
			//
			// lblEdu2
			//
			this.lblEdu2.AutoSize = true;
			this.lblEdu2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblEdu2.Location = new System.Drawing.Point(4, 80);
			this.lblEdu2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblEdu2.Name = "lblEdu2";
			this.lblEdu2.Size = new System.Drawing.Size(81, 20);
			this.lblEdu2.TabIndex = 18;
			this.lblEdu2.Text = "Save   - ";
			//
			// lblEduNotAvail
			//
			this.lblEduNotAvail.Location = new System.Drawing.Point(-19, 108);
			this.lblEduNotAvail.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblEduNotAvail.Name = "lblEduNotAvail";
			this.lblEduNotAvail.Size = new System.Drawing.Size(368, 69);
			this.lblEduNotAvail.TabIndex = 17;
			this.lblEduNotAvail.Text = "eduroam is not available at your current location.\r\n\r\nYour options are:";
			//
			// pbxStatus
			//
			this.pbxStatus.Image = global::EduroamApp.Properties.Resources.loading_gif;
			this.pbxStatus.Location = new System.Drawing.Point(251, 251);
			this.pbxStatus.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pbxStatus.Name = "pbxStatus";
			this.pbxStatus.Size = new System.Drawing.Size(16, 16);
			this.pbxStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pbxStatus.TabIndex = 9;
			this.pbxStatus.TabStop = false;
			this.pbxStatus.Visible = false;
			//
			// frmLogin
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(510, 553);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "frmLogin";
			this.Text = "frm6";
			this.Load += new System.EventHandler(this.frmLogin_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.pnlEduNotAvail.ResumeLayout(false);
			this.pnlEduNotAvail.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbxStatus)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtUsername;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label lblInst;
		private System.Windows.Forms.Label lblRules;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.PictureBox pbxStatus;
		private System.Windows.Forms.Label lblConnectFailed;
		private System.Windows.Forms.Panel pnlEduNotAvail;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lblEdu2;
		private System.Windows.Forms.Label lblEduNotAvail;
	}
}