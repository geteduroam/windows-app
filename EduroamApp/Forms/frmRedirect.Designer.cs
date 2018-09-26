namespace EduroamApp
{
	partial class frmRedirect
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
			this.lblRedirectLink = new System.Windows.Forms.LinkLabel();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(246, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Please see the following page for more information:";
			//
			// lblRedirectLink
			//
			this.lblRedirectLink.AutoSize = true;
			this.lblRedirectLink.Location = new System.Drawing.Point(3, 16);
			this.lblRedirectLink.MaximumSize = new System.Drawing.Size(375, 0);
			this.lblRedirectLink.Name = "lblRedirectLink";
			this.lblRedirectLink.Size = new System.Drawing.Size(64, 13);
			this.lblRedirectLink.TabIndex = 1;
			this.lblRedirectLink.TabStop = true;
			this.lblRedirectLink.Text = "redirectURL";
			this.lblRedirectLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblRedirectLink_LinkClicked);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 106);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(250, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "This application will close when you open the page.";
			//
			// frmRedirect
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(378, 246);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.lblRedirectLink);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "frmRedirect";
			this.Text = "frmRedirect";
			this.Load += new System.EventHandler(this.frmRedirect_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel lblRedirectLink;
		private System.Windows.Forms.Label label2;
	}
}