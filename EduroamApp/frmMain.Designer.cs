namespace EduroamApp
{
	partial class frmMain
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
			this.btnSelectProfile = new System.Windows.Forms.Button();
			this.txtOutput = new System.Windows.Forms.RichTextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnConnect = new System.Windows.Forms.Button();
			this.txtProfilePath = new System.Windows.Forms.TextBox();
			this.txtCertPwd = new System.Windows.Forms.TextBox();
			this.lblCertPwd = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.txtUsername = new System.Windows.Forms.TextBox();
			this.lblUsername = new System.Windows.Forms.Label();
			this.cboMethod = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.lblPassword = new System.Windows.Forms.Label();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.btnExit = new System.Windows.Forms.Button();
			this.btnTest = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			//
			// btnSelectProfile
			//
			this.btnSelectProfile.Enabled = false;
			this.btnSelectProfile.Location = new System.Drawing.Point(274, 241);
			this.btnSelectProfile.Name = "btnSelectProfile";
			this.btnSelectProfile.Size = new System.Drawing.Size(30, 23);
			this.btnSelectProfile.TabIndex = 0;
			this.btnSelectProfile.Text = "...";
			this.btnSelectProfile.UseVisualStyleBackColor = true;
			this.btnSelectProfile.Click += new System.EventHandler(this.btnSelectProfile_Click);
			//
			// txtOutput
			//
			this.txtOutput.Location = new System.Drawing.Point(16, 352);
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.ReadOnly = true;
			this.txtOutput.Size = new System.Drawing.Size(291, 126);
			this.txtOutput.TabIndex = 1;
			this.txtOutput.TabStop = false;
			this.txtOutput.Text = "";
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Enabled = false;
			this.label1.Location = new System.Drawing.Point(13, 246);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(143, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Select network profile (XML):";
			//
			// btnConnect
			//
			this.btnConnect.Location = new System.Drawing.Point(158, 284);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(145, 45);
			this.btnConnect.TabIndex = 2;
			this.btnConnect.Text = "Connect";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			//
			// txtProfilePath
			//
			this.txtProfilePath.Enabled = false;
			this.txtProfilePath.Location = new System.Drawing.Point(159, 243);
			this.txtProfilePath.Name = "txtProfilePath";
			this.txtProfilePath.ReadOnly = true;
			this.txtProfilePath.Size = new System.Drawing.Size(109, 20);
			this.txtProfilePath.TabIndex = 5;
			//
			// txtCertPwd
			//
			this.txtCertPwd.Location = new System.Drawing.Point(158, 136);
			this.txtCertPwd.Name = "txtCertPwd";
			this.txtCertPwd.Size = new System.Drawing.Size(109, 20);
			this.txtCertPwd.TabIndex = 1;
			this.txtCertPwd.Text = "eduroam";
			this.txtCertPwd.UseSystemPasswordChar = true;
			this.txtCertPwd.Visible = false;
			//
			// lblCertPwd
			//
			this.lblCertPwd.AutoSize = true;
			this.lblCertPwd.Location = new System.Drawing.Point(12, 139);
			this.lblCertPwd.Name = "lblCertPwd";
			this.lblCertPwd.Size = new System.Drawing.Size(132, 13);
			this.lblCertPwd.TabIndex = 6;
			this.lblCertPwd.Text = "Enter certificate password:";
			this.lblCertPwd.Visible = false;
			//
			// pictureBox1
			//
			this.pictureBox1.Image = global::EduroamApp.Properties.Resources.eduroam_logo;
			this.pictureBox1.Location = new System.Drawing.Point(182, 12);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(121, 58);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 8;
			this.pictureBox1.TabStop = false;
			//
			// txtUsername
			//
			this.txtUsername.Location = new System.Drawing.Point(159, 136);
			this.txtUsername.Name = "txtUsername";
			this.txtUsername.Size = new System.Drawing.Size(109, 20);
			this.txtUsername.TabIndex = 9;
			this.txtUsername.Text = "ericv@fyrkat.no";
			this.txtUsername.Visible = false;
			//
			// lblUsername
			//
			this.lblUsername.AutoSize = true;
			this.lblUsername.Location = new System.Drawing.Point(13, 139);
			this.lblUsername.Name = "lblUsername";
			this.lblUsername.Size = new System.Drawing.Size(58, 13);
			this.lblUsername.TabIndex = 10;
			this.lblUsername.Text = "Username:";
			this.lblUsername.Visible = false;
			//
			// cboMethod
			//
			this.cboMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboMethod.FormattingEnabled = true;
			this.cboMethod.Items.AddRange(new object[] {
			"Certificate",
			"Username and password"});
			this.cboMethod.Location = new System.Drawing.Point(158, 98);
			this.cboMethod.Name = "cboMethod";
			this.cboMethod.Size = new System.Drawing.Size(145, 21);
			this.cboMethod.TabIndex = 0;
			this.cboMethod.SelectedIndexChanged += new System.EventHandler(this.cboMethod_SelectedIndexChanged);
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(13, 101);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(140, 13);
			this.label4.TabIndex = 12;
			this.label4.Text = "Choose connection method:";
			//
			// lblPassword
			//
			this.lblPassword.AutoSize = true;
			this.lblPassword.Location = new System.Drawing.Point(13, 165);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(56, 13);
			this.lblPassword.TabIndex = 10;
			this.lblPassword.Text = "Password:";
			this.lblPassword.Visible = false;
			//
			// txtPassword
			//
			this.txtPassword.Location = new System.Drawing.Point(159, 162);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.Size = new System.Drawing.Size(109, 20);
			this.txtPassword.TabIndex = 9;
			this.txtPassword.Text = "eduroameduroam";
			this.txtPassword.UseSystemPasswordChar = true;
			this.txtPassword.Visible = false;
			//
			// btnExit
			//
			this.btnExit.Location = new System.Drawing.Point(243, 487);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(60, 23);
			this.btnExit.TabIndex = 13;
			this.btnExit.Text = "Exit";
			this.btnExit.UseVisualStyleBackColor = true;
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			//
			// btnTest
			//
			this.btnTest.Location = new System.Drawing.Point(40, 302);
			this.btnTest.Name = "btnTest";
			this.btnTest.Size = new System.Drawing.Size(75, 23);
			this.btnTest.TabIndex = 14;
			this.btnTest.Text = "Test cert";
			this.btnTest.UseVisualStyleBackColor = true;
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
			//
			// frmMain
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(319, 522);
			this.Controls.Add(this.btnTest);
			this.Controls.Add(this.btnExit);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.cboMethod);
			this.Controls.Add(this.txtPassword);
			this.Controls.Add(this.lblPassword);
			this.Controls.Add(this.lblUsername);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.lblCertPwd);
			this.Controls.Add(this.txtProfilePath);
			this.Controls.Add(this.btnConnect);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtOutput);
			this.Controls.Add(this.btnSelectProfile);
			this.Controls.Add(this.txtUsername);
			this.Controls.Add(this.txtCertPwd);
			this.Name = "frmMain";
			this.Text = "Eduroam installer";
			this.Load += new System.EventHandler(this.frmMain_load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnSelectProfile;
		private System.Windows.Forms.RichTextBox txtOutput;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.TextBox txtProfilePath;
		private System.Windows.Forms.TextBox txtCertPwd;
		private System.Windows.Forms.Label lblCertPwd;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TextBox txtUsername;
		private System.Windows.Forms.Label lblUsername;
		private System.Windows.Forms.ComboBox cboMethod;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Button btnExit;
		private System.Windows.Forms.Button btnTest;
	}
}

