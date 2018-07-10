namespace EduroamApp
{
	partial class Form1
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
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// btnSelectProfile
			//
			this.btnSelectProfile.Location = new System.Drawing.Point(276, 38);
			this.btnSelectProfile.Name = "btnSelectProfile";
			this.btnSelectProfile.Size = new System.Drawing.Size(30, 23);
			this.btnSelectProfile.TabIndex = 0;
			this.btnSelectProfile.Text = "...";
			this.btnSelectProfile.UseVisualStyleBackColor = true;
			this.btnSelectProfile.Click += new System.EventHandler(this.btnSelectProfile_Click);
			//
			// txtOutput
			//
			this.txtOutput.Location = new System.Drawing.Point(340, 12);
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.Size = new System.Drawing.Size(356, 249);
			this.txtOutput.TabIndex = 1;
			this.txtOutput.Text = "";
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 43);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(143, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Select network profile (XML):";
			//
			// btnConnect
			//
			this.btnConnect.Location = new System.Drawing.Point(113, 124);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(88, 45);
			this.btnConnect.TabIndex = 4;
			this.btnConnect.Text = "Connect to Eduroam";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			//
			// txtProfilePath
			//
			this.txtProfilePath.Location = new System.Drawing.Point(161, 40);
			this.txtProfilePath.Name = "txtProfilePath";
			this.txtProfilePath.Size = new System.Drawing.Size(109, 20);
			this.txtProfilePath.TabIndex = 5;
			//
			// txtCertPwd
			//
			this.txtCertPwd.Location = new System.Drawing.Point(161, 70);
			this.txtCertPwd.Name = "txtCertPwd";
			this.txtCertPwd.PasswordChar = '*';
			this.txtCertPwd.Size = new System.Drawing.Size(109, 20);
			this.txtCertPwd.TabIndex = 7;
			this.txtCertPwd.UseSystemPasswordChar = true;
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 73);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(132, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Enter certificate password:";
			//
			// Form1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(708, 273);
			this.Controls.Add(this.txtCertPwd);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtProfilePath);
			this.Controls.Add(this.btnConnect);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtOutput);
			this.Controls.Add(this.btnSelectProfile);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
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
		private System.Windows.Forms.Label label2;
	}
}

