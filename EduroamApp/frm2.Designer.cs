namespace EduroamApp
{
    partial class frm2
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
            this.btnDownloadEap = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblSelectProfile
            // 
            this.lblSelectProfile.AutoSize = true;
            this.lblSelectProfile.Enabled = false;
            this.lblSelectProfile.Location = new System.Drawing.Point(152, 177);
            this.lblSelectProfile.Name = "lblSelectProfile";
            this.lblSelectProfile.Size = new System.Drawing.Size(71, 13);
            this.lblSelectProfile.TabIndex = 27;
            this.lblSelectProfile.Text = "Select profile:";
            // 
            // cboProfiles
            // 
            this.cboProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProfiles.Enabled = false;
            this.cboProfiles.FormattingEnabled = true;
            this.cboProfiles.Location = new System.Drawing.Point(155, 193);
            this.cboProfiles.Name = "cboProfiles";
            this.cboProfiles.Size = new System.Drawing.Size(140, 21);
            this.cboProfiles.TabIndex = 26;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(152, 123);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 25;
            this.label2.Text = "Select institution:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(152, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "Select country:";
            // 
            // cboInstitution
            // 
            this.cboInstitution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInstitution.FormattingEnabled = true;
            this.cboInstitution.Location = new System.Drawing.Point(155, 139);
            this.cboInstitution.Name = "cboInstitution";
            this.cboInstitution.Size = new System.Drawing.Size(286, 21);
            this.cboInstitution.TabIndex = 23;
            // 
            // cboCountry
            // 
            this.cboCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCountry.FormattingEnabled = true;
            this.cboCountry.Location = new System.Drawing.Point(155, 89);
            this.cboCountry.Name = "cboCountry";
            this.cboCountry.Size = new System.Drawing.Size(140, 21);
            this.cboCountry.TabIndex = 22;
            // 
            // btnDownloadEap
            // 
            this.btnDownloadEap.Location = new System.Drawing.Point(155, 234);
            this.btnDownloadEap.Name = "btnDownloadEap";
            this.btnDownloadEap.Size = new System.Drawing.Size(105, 59);
            this.btnDownloadEap.TabIndex = 21;
            this.btnDownloadEap.Text = "Download config file and connect";
            this.btnDownloadEap.UseVisualStyleBackColor = true;
            // 
            // frm2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(592, 366);
            this.Controls.Add(this.lblSelectProfile);
            this.Controls.Add(this.cboProfiles);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboInstitution);
            this.Controls.Add(this.cboCountry);
            this.Controls.Add(this.btnDownloadEap);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frm2";
            this.Text = "frm2";
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
        private System.Windows.Forms.Button btnDownloadEap;
    }
}