namespace EduroamApp
{
	partial class frmTermsOfUse
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
			this.btnOk = new System.Windows.Forms.Button();
			this.txtToU = new System.Windows.Forms.RichTextBox();
			this.pnlOk = new System.Windows.Forms.Panel();
			this.pnlOk.SuspendLayout();
			this.SuspendLayout();
			//
			// btnOk
			//
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Location = new System.Drawing.Point(0, 0);
			this.btnOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(146, 45);
			this.btnOk.TabIndex = 1;
			this.btnOk.Text = "Ok";
			this.btnOk.UseVisualStyleBackColor = true;
			//
			// txtToU
			//
			this.txtToU.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.txtToU.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.txtToU.Location = new System.Drawing.Point(18, 18);
			this.txtToU.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtToU.Name = "txtToU";
			this.txtToU.ReadOnly = true;
			this.txtToU.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.txtToU.ShortcutsEnabled = false;
			this.txtToU.Size = new System.Drawing.Size(607, 337);
			this.txtToU.TabIndex = 2;
			this.txtToU.Text = "";
			this.txtToU.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.txtToU_LinkClicked);
			//
			// pnlOk
			//
			this.pnlOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlOk.Controls.Add(this.btnOk);
			this.pnlOk.Location = new System.Drawing.Point(542, 388);
			this.pnlOk.Name = "pnlOk";
			this.pnlOk.Size = new System.Drawing.Size(146, 45);
			this.pnlOk.TabIndex = 3;
			//
			// frmTermsOfUse
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(700, 445);
			this.ControlBox = false;
			this.Controls.Add(this.pnlOk);
			this.Controls.Add(this.txtToU);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.Name = "frmTermsOfUse";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "GetEduroam - Terms of Use";
			this.Load += new System.EventHandler(this.frmTermsOfUse_Load);
			this.pnlOk.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.RichTextBox txtToU;
		private System.Windows.Forms.Panel pnlOk;
	}
}