namespace EduroamApp
{
	partial class frmDownload
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
			this.lblSelectProfile = new System.Windows.Forms.Label();
			this.cboProfiles = new System.Windows.Forms.ComboBox();
			this.tlpLoading = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.lblError = new System.Windows.Forms.Label();
			this.lbInstitution = new System.Windows.Forms.ListBox();
			this.lblSearch = new System.Windows.Forms.Label();
			this.tbSearch = new System.Windows.Forms.TextBox();
			this.tlpLoading.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			//
			// lblSelectProfile
			//
			this.lblSelectProfile.AutoSize = true;
			this.lblSelectProfile.Location = new System.Drawing.Point(33, 297);
			this.lblSelectProfile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSelectProfile.Name = "lblSelectProfile";
			this.lblSelectProfile.Size = new System.Drawing.Size(105, 20);
			this.lblSelectProfile.TabIndex = 36;
			this.lblSelectProfile.Text = "Select profile:";
			this.lblSelectProfile.Visible = false;
			//
			// cboProfiles
			//
			this.cboProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboProfiles.FormattingEnabled = true;
			this.cboProfiles.Location = new System.Drawing.Point(167, 294);
			this.cboProfiles.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cboProfiles.Name = "cboProfiles";
			this.cboProfiles.Size = new System.Drawing.Size(301, 28);
			this.cboProfiles.TabIndex = 35;
			this.cboProfiles.Visible = false;
			this.cboProfiles.SelectedIndexChanged += new System.EventHandler(this.cboProfiles_SelectedIndexChanged);
			//
			// tlpLoading
			//
			this.tlpLoading.ColumnCount = 1;
			this.tlpLoading.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60.71429F));
			this.tlpLoading.Controls.Add(this.label1, 0, 0);
			this.tlpLoading.Controls.Add(this.pictureBox1, 0, 1);
			this.tlpLoading.Location = new System.Drawing.Point(240, 88);
			this.tlpLoading.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tlpLoading.Name = "tlpLoading";
			this.tlpLoading.RowCount = 2;
			this.tlpLoading.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tlpLoading.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
			this.tlpLoading.Size = new System.Drawing.Size(88, 68);
			this.tlpLoading.TabIndex = 38;
			//
			// label1
			//
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(11, 0);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(66, 20);
			this.label1.TabIndex = 38;
			this.label1.Text = "Loading";
			//
			// pictureBox1
			//
			this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.pictureBox1.Image = global::EduroamApp.Properties.Resources.loading_gif;
			this.pictureBox1.Location = new System.Drawing.Point(36, 31);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(16, 16);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 39;
			this.pictureBox1.TabStop = false;
			//
			// lblError
			//
			this.lblError.Location = new System.Drawing.Point(3, 3);
			this.lblError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblError.Name = "lblError";
			this.lblError.Size = new System.Drawing.Size(494, 89);
			this.lblError.TabIndex = 39;
			this.lblError.Text = "Couldn\'t connect to the server.\r\n\r\nMake sure that you are connected to the intern" +
	"et, then try again.";
			this.lblError.Visible = false;
			//
			// lbInstitution
			//
			this.lbInstitution.FormattingEnabled = true;
			this.lbInstitution.ItemHeight = 20;
			this.lbInstitution.Location = new System.Drawing.Point(37, 44);
			this.lbInstitution.Name = "lbInstitution";
			this.lbInstitution.Size = new System.Drawing.Size(485, 224);
			this.lbInstitution.TabIndex = 40;
			this.lbInstitution.SelectedIndexChanged += new System.EventHandler(this.lbInstitution_SelectedIndexChanged);
			//
			// lblSearch
			//
			this.lblSearch.AutoSize = true;
			this.lblSearch.Location = new System.Drawing.Point(33, 6);
			this.lblSearch.Name = "lblSearch";
			this.lblSearch.Size = new System.Drawing.Size(64, 20);
			this.lblSearch.TabIndex = 41;
			this.lblSearch.Text = "Search:";
			this.lblSearch.Click += new System.EventHandler(this.lblSearch_Click);
			//
			// tbSearch
			//
			this.tbSearch.Location = new System.Drawing.Point(103, 3);
			this.tbSearch.Name = "tbSearch";
			this.tbSearch.Size = new System.Drawing.Size(235, 26);
			this.tbSearch.TabIndex = 42;
			this.tbSearch.TextChanged += new System.EventHandler(this.tbSearch_TextChanged);
			//
			// frmDownload
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(567, 378);
			this.Controls.Add(this.tbSearch);
			this.Controls.Add(this.lblSearch);
			this.Controls.Add(this.lbInstitution);
			this.Controls.Add(this.tlpLoading);
			this.Controls.Add(this.lblSelectProfile);
			this.Controls.Add(this.cboProfiles);
			this.Controls.Add(this.lblError);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "frmDownload";
			this.Text = "frm3";
			this.Load += new System.EventHandler(this.frmDownload_Load);
			this.tlpLoading.ResumeLayout(false);
			this.tlpLoading.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label lblSelectProfile;
		private System.Windows.Forms.ComboBox cboProfiles;
		private System.Windows.Forms.TableLayoutPanel tlpLoading;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label lblError;
		private System.Windows.Forms.ListBox lbInstitution;
		private System.Windows.Forms.Label lblSearch;
		private System.Windows.Forms.TextBox tbSearch;
	}
}