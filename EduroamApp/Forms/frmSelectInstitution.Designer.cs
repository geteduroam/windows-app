namespace EduroamApp
{
    partial class frmSelectInstitution
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
            this.tlpLoading = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.lblError = new System.Windows.Forms.Label();
            this.lbInstitution = new System.Windows.Forms.ListBox();
            this.tbSearch = new System.Windows.Forms.TextBox();
            this.pbSearch = new System.Windows.Forms.PictureBox();
            this.tlpLoading.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSearch)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpLoading
            // 
            this.tlpLoading.ColumnCount = 1;
            this.tlpLoading.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60.71429F));
            this.tlpLoading.Controls.Add(this.label1, 0, 0);
            this.tlpLoading.Controls.Add(this.pbLoading, 0, 1);
            this.tlpLoading.Location = new System.Drawing.Point(148, 57);
            this.tlpLoading.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tlpLoading.Name = "tlpLoading";
            this.tlpLoading.RowCount = 3;
            this.tlpLoading.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpLoading.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.tlpLoading.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpLoading.Size = new System.Drawing.Size(183, 84);
            this.tlpLoading.TabIndex = 38;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(58, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 15);
            this.label1.TabIndex = 38;
            this.label1.Text = "Loading";
            // 
            // pbLoading
            // 
            this.pbLoading.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pbLoading.Image = global::EduroamApp.Properties.Resources.loading_gif;
            this.pbLoading.Location = new System.Drawing.Point(83, 20);
            this.pbLoading.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(16, 16);
            this.pbLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbLoading.TabIndex = 39;
            this.pbLoading.TabStop = false;
            // 
            // lblError
            // 
            this.lblError.Location = new System.Drawing.Point(25, 146);
            this.lblError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(457, 79);
            this.lblError.TabIndex = 39;
            this.lblError.Text = "Couldn\'t connect to the server.\r\n\r\nMake sure that you are connected to the intern" +
    "et, then try again.";
            this.lblError.Visible = false;
            // 
            // lbInstitution
            // 
            this.lbInstitution.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbInstitution.FormattingEnabled = true;
            this.lbInstitution.ItemHeight = 20;
            this.lbInstitution.Location = new System.Drawing.Point(10, 44);
            this.lbInstitution.Name = "lbInstitution";
            this.lbInstitution.Size = new System.Drawing.Size(484, 244);
            this.lbInstitution.TabIndex = 40;
            this.lbInstitution.SelectedIndexChanged += new System.EventHandler(this.lbInstitution_SelectedIndexChanged);
            this.lbInstitution.DoubleClick += new System.EventHandler(this.lbInstitution_DoubleClick);
            // 
            // tbSearch
            // 
            this.tbSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbSearch.Location = new System.Drawing.Point(10, 12);
            this.tbSearch.Name = "tbSearch";
            this.tbSearch.Size = new System.Drawing.Size(451, 26);
            this.tbSearch.TabIndex = 42;
            this.tbSearch.TextChanged += new System.EventHandler(this.tbSearch_TextChanged);
            // 
            // pbSearch
            // 
            this.pbSearch.Image = global::EduroamApp.Properties.Resources.searchImg;
            this.pbSearch.Location = new System.Drawing.Point(467, 12);
            this.pbSearch.Name = "pbSearch";
            this.pbSearch.Size = new System.Drawing.Size(27, 26);
            this.pbSearch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbSearch.TabIndex = 40;
            this.pbSearch.TabStop = false;
            // 
            // frmSelectInstitution
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(506, 549);
            this.Controls.Add(this.tlpLoading);
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.pbSearch);
            this.Controls.Add(this.tbSearch);
            this.Controls.Add(this.lbInstitution);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "frmSelectInstitution";
            this.Text = "frm3";
            this.Load += new System.EventHandler(this.frmSelectInstitution_Load);
            this.tlpLoading.ResumeLayout(false);
            this.tlpLoading.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSearch)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tlpLoading;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pbLoading;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.ListBox lbInstitution;
        private System.Windows.Forms.TextBox tbSearch;
        private System.Windows.Forms.PictureBox pbSearch;
    }
}