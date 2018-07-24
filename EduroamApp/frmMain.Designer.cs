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
			this.txtOutput = new System.Windows.Forms.RichTextBox();
			this.btnConnect = new System.Windows.Forms.Button();
			this.btnExit = new System.Windows.Forms.Button();
			this.btnTest = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.cboCountry = new System.Windows.Forms.ComboBox();
			this.cboInstitution = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.cboProfiles = new System.Windows.Forms.ComboBox();
			this.lblSelectProfile = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			//
			// txtOutput
			//
			this.txtOutput.Location = new System.Drawing.Point(17, 343);
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.ReadOnly = true;
			this.txtOutput.Size = new System.Drawing.Size(286, 126);
			this.txtOutput.TabIndex = 1;
			this.txtOutput.TabStop = false;
			this.txtOutput.Text = "";
			//
			// btnConnect
			//
			this.btnConnect.Location = new System.Drawing.Point(17, 292);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(145, 45);
			this.btnConnect.TabIndex = 2;
			this.btnConnect.Text = "Connect";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			//
			// btnExit
			//
			this.btnExit.Location = new System.Drawing.Point(243, 475);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(60, 23);
			this.btnExit.TabIndex = 13;
			this.btnExit.Text = "Exit";
			this.btnExit.UseVisualStyleBackColor = true;
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			//
			// btnTest
			//
			this.btnTest.Location = new System.Drawing.Point(17, 475);
			this.btnTest.Name = "btnTest";
			this.btnTest.Size = new System.Drawing.Size(75, 23);
			this.btnTest.TabIndex = 14;
			this.btnTest.Text = "Test";
			this.btnTest.UseVisualStyleBackColor = true;
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
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
			// cboCountry
			//
			this.cboCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboCountry.FormattingEnabled = true;
			this.cboCountry.Location = new System.Drawing.Point(17, 93);
			this.cboCountry.Name = "cboCountry";
			this.cboCountry.Size = new System.Drawing.Size(140, 21);
			this.cboCountry.TabIndex = 15;
			this.cboCountry.SelectedIndexChanged += new System.EventHandler(this.cboCountry_SelectedIndexChanged);
			//
			// cboInstitution
			//
			this.cboInstitution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboInstitution.FormattingEnabled = true;
			this.cboInstitution.Location = new System.Drawing.Point(17, 143);
			this.cboInstitution.Name = "cboInstitution";
			this.cboInstitution.Size = new System.Drawing.Size(286, 21);
			this.cboInstitution.TabIndex = 16;
			this.cboInstitution.SelectedIndexChanged += new System.EventHandler(this.cboInstitution_SelectedIndexChanged);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(14, 77);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(78, 13);
			this.label1.TabIndex = 17;
			this.label1.Text = "Select country:";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(14, 127);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(87, 13);
			this.label2.TabIndex = 18;
			this.label2.Text = "Select institution:";
			//
			// cboProfiles
			//
			this.cboProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboProfiles.Enabled = false;
			this.cboProfiles.FormattingEnabled = true;
			this.cboProfiles.Location = new System.Drawing.Point(17, 197);
			this.cboProfiles.Name = "cboProfiles";
			this.cboProfiles.Size = new System.Drawing.Size(140, 21);
			this.cboProfiles.TabIndex = 19;
			this.cboProfiles.SelectedIndexChanged += new System.EventHandler(this.cboProfiles_SelectedIndexChanged);
			//
			// lblSelectProfile
			//
			this.lblSelectProfile.AutoSize = true;
			this.lblSelectProfile.Enabled = false;
			this.lblSelectProfile.Location = new System.Drawing.Point(14, 181);
			this.lblSelectProfile.Name = "lblSelectProfile";
			this.lblSelectProfile.Size = new System.Drawing.Size(71, 13);
			this.lblSelectProfile.TabIndex = 20;
			this.lblSelectProfile.Text = "Select profile:";
			//
			// frmMain
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(319, 510);
			this.Controls.Add(this.lblSelectProfile);
			this.Controls.Add(this.cboProfiles);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cboInstitution);
			this.Controls.Add(this.cboCountry);
			this.Controls.Add(this.btnTest);
			this.Controls.Add(this.btnExit);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.btnConnect);
			this.Controls.Add(this.txtOutput);
			this.Name = "frmMain";
			this.Text = "Eduroam – installer";
			this.Load += new System.EventHandler(this.frmMain_load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.RichTextBox txtOutput;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button btnExit;
		private System.Windows.Forms.Button btnTest;
		private System.Windows.Forms.ComboBox cboCountry;
		private System.Windows.Forms.ComboBox cboInstitution;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cboProfiles;
		private System.Windows.Forms.Label lblSelectProfile;
	}
}

