namespace EduroamApp
{
    partial class frmParent
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
            this.pnlNavigation = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pnlNavBot = new System.Windows.Forms.Panel();
            this.pnlNavTop = new System.Windows.Forms.Panel();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.pnlLogoMid = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pnlLogoBot = new System.Windows.Forms.Panel();
            this.pnlLogoTop = new System.Windows.Forms.Panel();
            this.pnlLogoLeft = new System.Windows.Forms.Panel();
            this.pnlLogoRight = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblRedirect = new System.Windows.Forms.Label();
            this.lblLocalFileType = new System.Windows.Forms.Label();
            this.lblProfileCondition = new System.Windows.Forms.Label();
            this.lblInst = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlLeftMargin = new System.Windows.Forms.Panel();
            this.pnlRightMargin = new System.Windows.Forms.Panel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlNavigation.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.pnlLogoMid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlNavigation
            // 
            this.pnlNavigation.Controls.Add(this.tableLayoutPanel1);
            this.pnlNavigation.Controls.Add(this.panel2);
            this.pnlNavigation.Controls.Add(this.pnlNavBot);
            this.pnlNavigation.Controls.Add(this.pnlNavTop);
            this.pnlNavigation.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlNavigation.Location = new System.Drawing.Point(0, 366);
            this.pnlNavigation.Name = "pnlNavigation";
            this.pnlNavigation.Size = new System.Drawing.Size(608, 42);
            this.pnlNavigation.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.Controls.Add(this.btnCancel, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnBack, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnNext, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(370, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(232, 30);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCancel.Location = new System.Drawing.Point(157, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 24);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnBack
            // 
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBack.Location = new System.Drawing.Point(3, 3);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(71, 24);
            this.btnBack.TabIndex = 0;
            this.btnBack.Text = "< Back";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Visible = false;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // btnNext
            // 
            this.btnNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNext.Location = new System.Drawing.Point(80, 3);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(71, 24);
            this.btnNext.TabIndex = 0;
            this.btnNext.Text = "Install";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(602, 6);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(6, 30);
            this.panel2.TabIndex = 4;
            // 
            // pnlNavBot
            // 
            this.pnlNavBot.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlNavBot.Location = new System.Drawing.Point(0, 36);
            this.pnlNavBot.Name = "pnlNavBot";
            this.pnlNavBot.Size = new System.Drawing.Size(608, 6);
            this.pnlNavBot.TabIndex = 2;
            // 
            // pnlNavTop
            // 
            this.pnlNavTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlNavTop.Location = new System.Drawing.Point(0, 0);
            this.pnlNavTop.Name = "pnlNavTop";
            this.pnlNavTop.Size = new System.Drawing.Size(608, 6);
            this.pnlNavTop.TabIndex = 1;
            this.pnlNavTop.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlNavTop_Paint);
            // 
            // pnlLeft
            // 
            this.pnlLeft.Controls.Add(this.pnlLogoMid);
            this.pnlLeft.Controls.Add(this.pnlLogoBot);
            this.pnlLeft.Controls.Add(this.pnlLogoTop);
            this.pnlLeft.Controls.Add(this.pnlLogoLeft);
            this.pnlLeft.Controls.Add(this.pnlLogoRight);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(160, 366);
            this.pnlLeft.TabIndex = 2;
            // 
            // pnlLogoMid
            // 
            this.pnlLogoMid.Controls.Add(this.pictureBox1);
            this.pnlLogoMid.Controls.Add(this.pictureBox2);
            this.pnlLogoMid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLogoMid.Location = new System.Drawing.Point(8, 59);
            this.pnlLogoMid.Name = "pnlLogoMid";
            this.pnlLogoMid.Size = new System.Drawing.Size(144, 241);
            this.pnlLogoMid.TabIndex = 7;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox1.Image = global::EduroamApp.Properties.Resources.eduroam_logo_400px;
            this.pictureBox1.Location = new System.Drawing.Point(0, 76);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(144, 65);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox2.Image = global::EduroamApp.Properties.Resources._1280px_Uninett_logo_svg;
            this.pictureBox2.Location = new System.Drawing.Point(0, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(144, 76);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // pnlLogoBot
            // 
            this.pnlLogoBot.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLogoBot.Location = new System.Drawing.Point(8, 300);
            this.pnlLogoBot.Name = "pnlLogoBot";
            this.pnlLogoBot.Size = new System.Drawing.Size(144, 66);
            this.pnlLogoBot.TabIndex = 6;
            // 
            // pnlLogoTop
            // 
            this.pnlLogoTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLogoTop.Location = new System.Drawing.Point(8, 0);
            this.pnlLogoTop.Name = "pnlLogoTop";
            this.pnlLogoTop.Size = new System.Drawing.Size(144, 59);
            this.pnlLogoTop.TabIndex = 5;
            // 
            // pnlLogoLeft
            // 
            this.pnlLogoLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLogoLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLogoLeft.Name = "pnlLogoLeft";
            this.pnlLogoLeft.Size = new System.Drawing.Size(8, 366);
            this.pnlLogoLeft.TabIndex = 3;
            // 
            // pnlLogoRight
            // 
            this.pnlLogoRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlLogoRight.Location = new System.Drawing.Point(152, 0);
            this.pnlLogoRight.Name = "pnlLogoRight";
            this.pnlLogoRight.Size = new System.Drawing.Size(8, 366);
            this.pnlLogoRight.TabIndex = 2;
            this.pnlLogoRight.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlLogoRight_Paint);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.lblRedirect);
            this.panel1.Controls.Add(this.lblLocalFileType);
            this.panel1.Controls.Add(this.lblProfileCondition);
            this.panel1.Controls.Add(this.lblInst);
            this.panel1.Controls.Add(this.lblTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(160, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(448, 120);
            this.panel1.TabIndex = 4;
            // 
            // lblRedirect
            // 
            this.lblRedirect.AutoSize = true;
            this.lblRedirect.Location = new System.Drawing.Point(330, 53);
            this.lblRedirect.Name = "lblRedirect";
            this.lblRedirect.Size = new System.Drawing.Size(84, 13);
            this.lblRedirect.TabIndex = 11;
            this.lblRedirect.Text = "label: redirect url";
            this.lblRedirect.Visible = false;
            // 
            // lblLocalFileType
            // 
            this.lblLocalFileType.AutoSize = true;
            this.lblLocalFileType.Location = new System.Drawing.Point(330, 40);
            this.lblLocalFileType.Name = "lblLocalFileType";
            this.lblLocalFileType.Size = new System.Drawing.Size(96, 13);
            this.lblLocalFileType.TabIndex = 10;
            this.lblLocalFileType.Text = "label: local file type";
            this.lblLocalFileType.Visible = false;
            // 
            // lblProfileCondition
            // 
            this.lblProfileCondition.AutoSize = true;
            this.lblProfileCondition.Location = new System.Drawing.Point(330, 27);
            this.lblProfileCondition.Name = "lblProfileCondition";
            this.lblProfileCondition.Size = new System.Drawing.Size(109, 13);
            this.lblProfileCondition.TabIndex = 9;
            this.lblProfileCondition.Text = "label: profile condition";
            this.lblProfileCondition.Visible = false;
            // 
            // lblInst
            // 
            this.lblInst.AutoSize = true;
            this.lblInst.Location = new System.Drawing.Point(330, 14);
            this.lblInst.Name = "lblInst";
            this.lblInst.Size = new System.Drawing.Size(108, 13);
            this.lblInst.TabIndex = 8;
            this.lblInst.Text = "label: institution name";
            this.lblInst.Visible = false;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(35, 59);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(138, 20);
            this.lblTitle.TabIndex = 7;
            this.lblTitle.Text = "Connection status";
            // 
            // pnlLeftMargin
            // 
            this.pnlLeftMargin.BackColor = System.Drawing.Color.White;
            this.pnlLeftMargin.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeftMargin.Location = new System.Drawing.Point(160, 120);
            this.pnlLeftMargin.Name = "pnlLeftMargin";
            this.pnlLeftMargin.Size = new System.Drawing.Size(35, 246);
            this.pnlLeftMargin.TabIndex = 5;
            // 
            // pnlRightMargin
            // 
            this.pnlRightMargin.BackColor = System.Drawing.Color.White;
            this.pnlRightMargin.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlRightMargin.Location = new System.Drawing.Point(573, 120);
            this.pnlRightMargin.Name = "pnlRightMargin";
            this.pnlRightMargin.Size = new System.Drawing.Size(35, 246);
            this.pnlRightMargin.TabIndex = 6;
            // 
            // pnlContent
            // 
            this.pnlContent.AutoSize = true;
            this.pnlContent.BackColor = System.Drawing.Color.White;
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(195, 120);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(378, 246);
            this.pnlContent.TabIndex = 7;
            // 
            // frmParent
            // 
            this.AcceptButton = this.btnNext;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 408);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlRightMargin);
            this.Controls.Add(this.pnlLeftMargin);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlNavigation);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmParent";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Eduroam installer";
            this.Load += new System.EventHandler(this.frmParent_Load);
            this.pnlNavigation.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlLogoMid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlNavigation;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlLeftMargin;
        private System.Windows.Forms.Panel pnlRightMargin;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Label lblInst;
        private System.Windows.Forms.Panel pnlNavTop;
        private System.Windows.Forms.Panel pnlNavBot;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel pnlLogoLeft;
        private System.Windows.Forms.Panel pnlLogoRight;
        private System.Windows.Forms.Panel pnlLogoMid;
        private System.Windows.Forms.Panel pnlLogoBot;
        private System.Windows.Forms.Panel pnlLogoTop;
        private System.Windows.Forms.Label lblProfileCondition;
        private System.Windows.Forms.Label lblLocalFileType;
        private System.Windows.Forms.Label lblRedirect;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}