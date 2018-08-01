namespace EduroamApp
{
	partial class frmSelectMethod
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSelectMethod));
			this.rdbDownload = new System.Windows.Forms.RadioButton();
			this.rdbLocal = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// rdbDownload
			//
			this.rdbDownload.AutoSize = true;
			this.rdbDownload.Checked = true;
			this.rdbDownload.Location = new System.Drawing.Point(4, 78);
			this.rdbDownload.Name = "rdbDownload";
			this.rdbDownload.Size = new System.Drawing.Size(197, 17);
			this.rdbDownload.TabIndex = 0;
			this.rdbDownload.TabStop = true;
			this.rdbDownload.Text = "Automatic download (recommended)";
			this.rdbDownload.UseVisualStyleBackColor = true;
			this.rdbDownload.CheckedChanged += new System.EventHandler(this.rdbDownload_CheckedChanged);
			//
			// rdbLocal
			//
			this.rdbLocal.AutoSize = true;
			this.rdbLocal.Location = new System.Drawing.Point(4, 101);
			this.rdbLocal.Name = "rdbLocal";
			this.rdbLocal.Size = new System.Drawing.Size(128, 17);
			this.rdbLocal.TabIndex = 1;
			this.rdbLocal.Text = "Select local config file";
			this.rdbLocal.UseVisualStyleBackColor = true;
			//
			// label1
			//
			this.label1.Location = new System.Drawing.Point(1, 2);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(377, 73);
			this.label1.TabIndex = 2;
			this.label1.Text = resources.GetString("label1.Text");
			//
			// frm2
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(378, 246);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.rdbLocal);
			this.Controls.Add(this.rdbDownload);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "frmSelectMethod";
			this.Text = "frm2";
			this.Load += new System.EventHandler(this.frm2_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RadioButton rdbDownload;
		private System.Windows.Forms.RadioButton rdbLocal;
		private System.Windows.Forms.Label label1;
	}
}