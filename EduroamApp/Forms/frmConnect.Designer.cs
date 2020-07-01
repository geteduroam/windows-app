namespace EduroamApp
{
    partial class frmConnect
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
            this.lblStatus = new System.Windows.Forms.Label();
            this.tblConnectStatus = new System.Windows.Forms.TableLayoutPanel();
            this.pbxStatus = new System.Windows.Forms.PictureBox();
            this.lblConnectFailed = new System.Windows.Forms.Label();
            this.pnlEduNotAvail = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblEdu2 = new System.Windows.Forms.Label();
            this.lblEduNotAvail = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbxStatus)).BeginInit();
            this.pnlEduNotAvail.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStatus.Location = new System.Drawing.Point(35, 123);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(450, 63);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "Connecting...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStatus.Visible = false;
            // 
            // tblConnectStatus
            // 
            this.tblConnectStatus.AutoSize = true;
            this.tblConnectStatus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tblConnectStatus.ColumnCount = 2;
            this.tblConnectStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblConnectStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 256F));
            this.tblConnectStatus.Location = new System.Drawing.Point(-2, 18);
            this.tblConnectStatus.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tblConnectStatus.Name = "tblConnectStatus";
            this.tblConnectStatus.RowCount = 1;
            this.tblConnectStatus.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblConnectStatus.Size = new System.Drawing.Size(256, 0);
            this.tblConnectStatus.TabIndex = 9;
            // 
            // pbxStatus
            // 
            this.pbxStatus.Image = global::EduroamApp.Properties.Resources.loading_gif;
            this.pbxStatus.Location = new System.Drawing.Point(238, 28);
            this.pbxStatus.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbxStatus.Name = "pbxStatus";
            this.pbxStatus.Size = new System.Drawing.Size(16, 16);
            this.pbxStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbxStatus.TabIndex = 8;
            this.pbxStatus.TabStop = false;
            this.pbxStatus.Visible = false;
            // 
            // lblConnectFailed
            // 
            this.lblConnectFailed.Location = new System.Drawing.Point(24, -31);
            this.lblConnectFailed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConnectFailed.Name = "lblConnectFailed";
            this.lblConnectFailed.Size = new System.Drawing.Size(415, 78);
            this.lblConnectFailed.TabIndex = 10;
            this.lblConnectFailed.Text = "Press Back if you want to try again, connect to a different institution or select" +
    " a different config file.";
            this.lblConnectFailed.Visible = false;
            // 
            // pnlEduNotAvail
            // 
            this.pnlEduNotAvail.Controls.Add(this.label2);
            this.pnlEduNotAvail.Controls.Add(this.lblConnectFailed);
            this.pnlEduNotAvail.Controls.Add(this.label3);
            this.pnlEduNotAvail.Controls.Add(this.label1);
            this.pnlEduNotAvail.Controls.Add(this.lblEdu2);
            this.pnlEduNotAvail.Controls.Add(this.lblEduNotAvail);
            this.pnlEduNotAvail.Location = new System.Drawing.Point(11, 217);
            this.pnlEduNotAvail.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlEduNotAvail.Name = "pnlEduNotAvail";
            this.pnlEduNotAvail.Size = new System.Drawing.Size(457, 206);
            this.pnlEduNotAvail.TabIndex = 17;
            this.pnlEduNotAvail.Visible = false;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(87, 157);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(312, 52);
            this.label2.TabIndex = 21;
            this.label2.Text = "Delete your configuration and exit the application.\r\n";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(3, 157);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 20);
            this.label3.TabIndex = 20;
            this.label3.Text = "Cancel - ";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(87, 80);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(370, 78);
            this.label1.TabIndex = 19;
            this.label1.Text = "Save your current configuration.\r\nYou will be able to automatically connect to ed" +
    "uroam when it becomes available.";
            // 
            // lblEdu2
            // 
            this.lblEdu2.AutoSize = true;
            this.lblEdu2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEdu2.Location = new System.Drawing.Point(4, 80);
            this.lblEdu2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEdu2.Name = "lblEdu2";
            this.lblEdu2.Size = new System.Drawing.Size(81, 20);
            this.lblEdu2.TabIndex = 18;
            this.lblEdu2.Text = "Save   - ";
            // 
            // lblEduNotAvail
            // 
            this.lblEduNotAvail.Location = new System.Drawing.Point(9, 0);
            this.lblEduNotAvail.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEduNotAvail.Name = "lblEduNotAvail";
            this.lblEduNotAvail.Size = new System.Drawing.Size(368, 69);
            this.lblEduNotAvail.TabIndex = 17;
            this.lblEduNotAvail.Text = "eduroam is not available at your current location.\r\n\r\nYour options are:";
            // 
            // frmConnect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(509, 465);
            this.Controls.Add(this.pbxStatus);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.tblConnectStatus);
            this.Controls.Add(this.pnlEduNotAvail);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "frmConnect";
            this.Text = "frm5";
            this.Load += new System.EventHandler(this.frmConnect_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbxStatus)).EndInit();
            this.pnlEduNotAvail.ResumeLayout(false);
            this.pnlEduNotAvail.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.PictureBox pbxStatus;
        private System.Windows.Forms.TableLayoutPanel tblConnectStatus;
        private System.Windows.Forms.Label lblConnectFailed;
        private System.Windows.Forms.Panel pnlEduNotAvail;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblEdu2;
        private System.Windows.Forms.Label lblEduNotAvail;
    }
}