namespace EduroamApp
{
	partial class frmLocal
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
			this.btnBrowse = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.txtFilepath = new System.Windows.Forms.TextBox();
			this.txtCertPassword = new System.Windows.Forms.TextBox();
			this.lblCertPassword = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// btnBrowse
			//
			this.btnBrowse.Location = new System.Drawing.Point(263, 17);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(73, 23);
			this.btnBrowse.TabIndex = 3;
			this.btnBrowse.Text = "Browse...";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(55, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "File name:";
			//
			// txtFilepath
			//
			this.txtFilepath.Location = new System.Drawing.Point(6, 19);
			this.txtFilepath.Name = "txtFilepath";
			this.txtFilepath.Size = new System.Drawing.Size(251, 20);
			this.txtFilepath.TabIndex = 4;
			this.txtFilepath.TextChanged += new System.EventHandler(this.txtFilepath_TextChanged);
			//
			// txtCertPassword
			//
			this.txtCertPassword.Location = new System.Drawing.Point(6, 86);
			this.txtCertPassword.Name = "txtCertPassword";
			this.txtCertPassword.Size = new System.Drawing.Size(182, 20);
			this.txtCertPassword.TabIndex = 6;
			this.txtCertPassword.UseSystemPasswordChar = true;
			this.txtCertPassword.Visible = false;
			//
			// lblCertPassword
			//
			this.lblCertPassword.AutoSize = true;
			this.lblCertPassword.Location = new System.Drawing.Point(3, 70);
			this.lblCertPassword.Name = "lblCertPassword";
			this.lblCertPassword.Size = new System.Drawing.Size(105, 13);
			this.lblCertPassword.TabIndex = 5;
			this.lblCertPassword.Text = "Certificate password:";
			this.lblCertPassword.Visible = false;
			//
			// frmLocal
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(378, 246);
			this.Controls.Add(this.txtCertPassword);
			this.Controls.Add(this.lblCertPassword);
			this.Controls.Add(this.txtFilepath);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "frmLocal";
			this.Text = "frm4";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtFilepath;
		private System.Windows.Forms.TextBox txtCertPassword;
		private System.Windows.Forms.Label lblCertPassword;
	}
}