namespace EduroamApp
{
    partial class frmSelectProfile
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
            this.lblError = new System.Windows.Forms.Label();
            this.lbProfile = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lblError
            // 
            this.lblError.Location = new System.Drawing.Point(13, 173);
            this.lblError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(494, 89);
            this.lblError.TabIndex = 39;
            this.lblError.Text = "Couldn\'t connect to the server.\r\n\r\nMake sure that you are connected to the intern" +
    "et, then try again.";
            this.lblError.Visible = false;
            // 
            // lbProfile
            // 
            this.lbProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbProfile.FormattingEnabled = true;
            this.lbProfile.ItemHeight = 20;
            this.lbProfile.Location = new System.Drawing.Point(12, 12);
            this.lbProfile.Name = "lbProfile";
            this.lbProfile.Size = new System.Drawing.Size(484, 284);
            this.lbProfile.TabIndex = 40;
            this.lbProfile.SelectedIndexChanged += new System.EventHandler(this.lbProfile_SelectedIndexChanged);
            // 
            // frmSelectProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(506, 745);
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.lbProfile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "frmSelectProfile";
            this.Text = "frm3";
            this.Load += new System.EventHandler(this.frmSelectProfile_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.ListBox lbProfile;
    }
}