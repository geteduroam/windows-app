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
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 13);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(260, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "Enter your username and password";
			//
			// txtUsername
			//
			this.txtUsername.ForeColor = System.Drawing.SystemColors.GrayText;
			this.txtUsername.Location = new System.Drawing.Point(13, 38);
			this.txtUsername.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtUsername.Name = "txtUsername";
			this.txtUsername.Size = new System.Drawing.Size(268, 26);
			this.txtUsername.TabIndex = 1;
			this.txtUsername.Text = "Username";
			this.txtUsername.TextChanged += new System.EventHandler(this.txtUsername_TextChanged);
			this.txtUsername.Enter += new System.EventHandler(this.txtUsername_Enter);
			this.txtUsername.Leave += new System.EventHandler(this.txtUsername_Leave);
			//
			// txtPassword
			//
			this.txtPassword.ForeColor = System.Drawing.SystemColors.GrayText;
			this.txtPassword.Location = new System.Drawing.Point(13, 74);
			this.txtPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.Size = new System.Drawing.Size(268, 26);
			this.txtPassword.TabIndex = 2;
			this.txtPassword.Text = "Password";
			this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
			this.txtPassword.Enter += new System.EventHandler(this.txtPassword_Enter);
			this.txtPassword.Leave += new System.EventHandler(this.txtPassword_Leave);
			//
			// lblInst
			//
			this.lblInst.AutoSize = true;
			this.lblInst.Location = new System.Drawing.Point(289, 41);
			this.lblInst.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblInst.Name = "lblInst";
			this.lblInst.Size = new System.Drawing.Size(127, 20);
			this.lblInst.TabIndex = 0;
			this.lblInst.Text = "@institution.com";
			this.lblInst.Visible = false;
			//
			// lblRules
			//
			this.lblRules.Location = new System.Drawing.Point(9, 105);
			this.lblRules.Name = "lblRules";
			this.lblRules.Size = new System.Drawing.Size(393, 122);
			this.lblRules.TabIndex = 3;
			//
			// panel1
			//
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.lblRules);
			this.panel1.Controls.Add(this.txtUsername);
			this.panel1.Controls.Add(this.lblInst);
			this.panel1.Controls.Add(this.txtPassword);
			this.panel1.Location = new System.Drawing.Point(82, 69);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(521, 243);
			this.panel1.TabIndex = 4;
			//
			// frmLogin
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(608, 431);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "frmLogin";
			this.Text = "frm6";
			this.Load += new System.EventHandler(this.frm6_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtUsername;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label lblInst;
		private System.Windows.Forms.Label lblRules;
		private System.Windows.Forms.Panel panel1;
	}
}