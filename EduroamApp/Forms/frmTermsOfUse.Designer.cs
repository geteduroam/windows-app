namespace EduroamApp
{
    partial class frmTermsOfUse
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
            this.btnOk = new System.Windows.Forms.Button();
            this.txtToU = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(325, 238);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(87, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // txtToU
            // 
            this.txtToU.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtToU.Location = new System.Drawing.Point(12, 12);
            this.txtToU.Name = "txtToU";
            this.txtToU.ReadOnly = true;
            this.txtToU.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtToU.ShortcutsEnabled = false;
            this.txtToU.Size = new System.Drawing.Size(400, 220);
            this.txtToU.TabIndex = 2;
            this.txtToU.Text = "";
            // 
            // frmTermsOfUse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 270);
            this.Controls.Add(this.txtToU);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmTermsOfUse";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "eduroam - Terms of Use";
            this.Load += new System.EventHandler(this.frmTermsOfUse_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.RichTextBox txtToU;
    }
}