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
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(173, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Enter your username and password";
			//
			// txtUsername
			//
			this.txtUsername.ForeColor = System.Drawing.SystemColors.GrayText;
			this.txtUsername.Location = new System.Drawing.Point(6, 24);
			this.txtUsername.Name = "txtUsername";
			this.txtUsername.Size = new System.Drawing.Size(206, 20);
			this.txtUsername.TabIndex = 1;
			this.txtUsername.Text = "Username";
			this.txtUsername.Enter += new System.EventHandler(this.txtUsername_Enter);
			this.txtUsername.Leave += new System.EventHandler(this.txtUsername_Leave);
			//
			// txtPassword
			//
			this.txtPassword.ForeColor = System.Drawing.SystemColors.GrayText;
			this.txtPassword.Location = new System.Drawing.Point(6, 50);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.Size = new System.Drawing.Size(206, 20);
			this.txtPassword.TabIndex = 2;
			this.txtPassword.Text = "Password";
			this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
			this.txtPassword.Enter += new System.EventHandler(this.txtPassword_Enter);
			this.txtPassword.Leave += new System.EventHandler(this.txtPassword_Leave);
			//
			// lblInst
			//
			this.lblInst.AutoSize = true;
			this.lblInst.Location = new System.Drawing.Point(211, 27);
			this.lblInst.Name = "lblInst";
			this.lblInst.Size = new System.Drawing.Size(85, 13);
			this.lblInst.TabIndex = 0;
			this.lblInst.Text = "@institution.com";
			this.lblInst.Visible = false;
			//
			// frmLogin
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(378, 246);
			this.Controls.Add(this.txtPassword);
			this.Controls.Add(this.txtUsername);
			this.Controls.Add(this.lblInst);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "frmLogin";
			this.Text = "frm6";
			this.Load += new System.EventHandler(this.frm6_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtUsername;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label lblInst;
	}
}