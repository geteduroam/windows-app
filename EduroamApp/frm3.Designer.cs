namespace EduroamApp
{
	partial class frm3
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
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.cboInstitution = new System.Windows.Forms.ComboBox();
			this.cboCountry = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// lblSelectProfile
			//
			this.lblSelectProfile.AutoSize = true;
			this.lblSelectProfile.Enabled = false;
			this.lblSelectProfile.Location = new System.Drawing.Point(46, 225);
			this.lblSelectProfile.Name = "lblSelectProfile";
			this.lblSelectProfile.Size = new System.Drawing.Size(71, 13);
			this.lblSelectProfile.TabIndex = 36;
			this.lblSelectProfile.Text = "Select profile:";
			//
			// cboProfiles
			//
			this.cboProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboProfiles.Enabled = false;
			this.cboProfiles.FormattingEnabled = true;
			this.cboProfiles.Location = new System.Drawing.Point(49, 241);
			this.cboProfiles.Name = "cboProfiles";
			this.cboProfiles.Size = new System.Drawing.Size(202, 21);
			this.cboProfiles.TabIndex = 35;
			this.cboProfiles.SelectedIndexChanged += new System.EventHandler(this.cboProfiles_SelectedIndexChanged);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(46, 171);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(87, 13);
			this.label2.TabIndex = 34;
			this.label2.Text = "Select institution:";
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(46, 121);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(78, 13);
			this.label1.TabIndex = 33;
			this.label1.Text = "Select country:";
			//
			// cboInstitution
			//
			this.cboInstitution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboInstitution.FormattingEnabled = true;
			this.cboInstitution.Location = new System.Drawing.Point(49, 187);
			this.cboInstitution.Name = "cboInstitution";
			this.cboInstitution.Size = new System.Drawing.Size(286, 21);
			this.cboInstitution.TabIndex = 32;
			this.cboInstitution.SelectedIndexChanged += new System.EventHandler(this.cboInstitution_SelectedIndexChanged);
			//
			// cboCountry
			//
			this.cboCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboCountry.FormattingEnabled = true;
			this.cboCountry.Location = new System.Drawing.Point(49, 137);
			this.cboCountry.Name = "cboCountry";
			this.cboCountry.Size = new System.Drawing.Size(140, 21);
			this.cboCountry.TabIndex = 31;
			this.cboCountry.SelectedIndexChanged += new System.EventHandler(this.cboCountry_SelectedIndexChanged);
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(45, 43);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(160, 20);
			this.label3.TabIndex = 37;
			this.label3.Text = "Select your institution";
			//
			// frm3
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(448, 366);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.lblSelectProfile);
			this.Controls.Add(this.cboProfiles);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cboInstitution);
			this.Controls.Add(this.cboCountry);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "frm3";
			this.Text = "frm3";
			this.Load += new System.EventHandler(this.frm3_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label lblSelectProfile;
		private System.Windows.Forms.ComboBox cboProfiles;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cboInstitution;
		private System.Windows.Forms.ComboBox cboCountry;
		private System.Windows.Forms.Label label3;
	}
}