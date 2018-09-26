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
            this.lblInstitution = new System.Windows.Forms.Label();
            this.lblCountry = new System.Windows.Forms.Label();
            this.cboInstitution = new System.Windows.Forms.ComboBox();
            this.cboCountry = new System.Windows.Forms.ComboBox();
            this.tlpLoading = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblError = new System.Windows.Forms.Label();
            this.tlpLoading.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblSelectProfile
            // 
            this.lblSelectProfile.AutoSize = true;
            this.lblSelectProfile.Location = new System.Drawing.Point(2, 107);
            this.lblSelectProfile.Name = "lblSelectProfile";
            this.lblSelectProfile.Size = new System.Drawing.Size(71, 13);
            this.lblSelectProfile.TabIndex = 36;
            this.lblSelectProfile.Text = "Select profile:";
            this.lblSelectProfile.Visible = false;
            // 
            // cboProfiles
            // 
            this.cboProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProfiles.FormattingEnabled = true;
            this.cboProfiles.Location = new System.Drawing.Point(5, 123);
            this.cboProfiles.Name = "cboProfiles";
            this.cboProfiles.Size = new System.Drawing.Size(202, 21);
            this.cboProfiles.TabIndex = 35;
            this.cboProfiles.Visible = false;
            this.cboProfiles.SelectedIndexChanged += new System.EventHandler(this.cboProfiles_SelectedIndexChanged);
            // 
            // lblInstitution
            // 
            this.lblInstitution.AutoSize = true;
            this.lblInstitution.Location = new System.Drawing.Point(2, 53);
            this.lblInstitution.Name = "lblInstitution";
            this.lblInstitution.Size = new System.Drawing.Size(87, 13);
            this.lblInstitution.TabIndex = 34;
            this.lblInstitution.Text = "Select institution:";
            // 
            // lblCountry
            // 
            this.lblCountry.AutoSize = true;
            this.lblCountry.Location = new System.Drawing.Point(2, 3);
            this.lblCountry.Name = "lblCountry";
            this.lblCountry.Size = new System.Drawing.Size(78, 13);
            this.lblCountry.TabIndex = 33;
            this.lblCountry.Text = "Select country:";
            // 
            // cboInstitution
            // 
            this.cboInstitution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInstitution.FormattingEnabled = true;
            this.cboInstitution.Location = new System.Drawing.Point(5, 69);
            this.cboInstitution.Name = "cboInstitution";
            this.cboInstitution.Size = new System.Drawing.Size(286, 21);
            this.cboInstitution.TabIndex = 32;
            this.cboInstitution.SelectedIndexChanged += new System.EventHandler(this.cboInstitution_SelectedIndexChanged);
            // 
            // cboCountry
            // 
            this.cboCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCountry.FormattingEnabled = true;
            this.cboCountry.Location = new System.Drawing.Point(5, 19);
            this.cboCountry.Name = "cboCountry";
            this.cboCountry.Size = new System.Drawing.Size(202, 21);
            this.cboCountry.TabIndex = 31;
            this.cboCountry.SelectedIndexChanged += new System.EventHandler(this.cboCountry_SelectedIndexChanged);
            // 
            // tlpLoading
            // 
            this.tlpLoading.ColumnCount = 1;
            this.tlpLoading.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60.71429F));
            this.tlpLoading.Controls.Add(this.label1, 0, 0);
            this.tlpLoading.Controls.Add(this.pictureBox1, 0, 1);
            this.tlpLoading.Location = new System.Drawing.Point(160, 57);
            this.tlpLoading.Name = "tlpLoading";
            this.tlpLoading.RowCount = 2;
            this.tlpLoading.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpLoading.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tlpLoading.Size = new System.Drawing.Size(59, 44);
            this.tlpLoading.TabIndex = 38;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 38;
            this.label1.Text = "Loading";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pictureBox1.Image = global::EduroamApp.Properties.Resources.loading_gif;
            this.pictureBox1.Location = new System.Drawing.Point(21, 20);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 39;
            this.pictureBox1.TabStop = false;
            // 
            // lblError
            // 
            this.lblError.Location = new System.Drawing.Point(2, 2);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(329, 58);
            this.lblError.TabIndex = 39;
            this.lblError.Text = "Couldn\'t connect to the server.\r\n\r\nMake sure that you are connected to the intern" +
    "et, then try again.";
            this.lblError.Visible = false;
            // 
            // frmDownload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(378, 246);
            this.Controls.Add(this.tlpLoading);
            this.Controls.Add(this.lblSelectProfile);
            this.Controls.Add(this.cboProfiles);
            this.Controls.Add(this.lblInstitution);
            this.Controls.Add(this.lblCountry);
            this.Controls.Add(this.cboInstitution);
            this.Controls.Add(this.cboCountry);
            this.Controls.Add(this.lblError);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
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
        private System.Windows.Forms.Label lblInstitution;
        private System.Windows.Forms.Label lblCountry;
        private System.Windows.Forms.ComboBox cboInstitution;
        private System.Windows.Forms.ComboBox cboCountry;
        private System.Windows.Forms.TableLayoutPanel tlpLoading;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblError;
    }
}