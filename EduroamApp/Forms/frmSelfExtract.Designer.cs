namespace EduroamApp
{
	partial class frmSelfExtract
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
			this.label1 = new System.Windows.Forms.Label();
			this.btnAltSetup = new System.Windows.Forms.Button();
			this.pnlAltPopup = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.lblInstName = new System.Windows.Forms.Label();
			this.lblWeb = new System.Windows.Forms.Label();
			this.lblEmail = new System.Windows.Forms.Label();
			this.lblPhone = new System.Windows.Forms.Label();
			this.tblContactInfo = new System.Windows.Forms.TableLayoutPanel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.pnlAltPopup.SuspendLayout();
			this.tblContactInfo.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(2, 169);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(122, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Press Install to continue.";
			//
			// btnAltSetup
			//
			this.btnAltSetup.BackColor = System.Drawing.SystemColors.ControlLight;
			this.btnAltSetup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnAltSetup.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.btnAltSetup.FlatAppearance.BorderSize = 2;
			this.btnAltSetup.Location = new System.Drawing.Point(0, 0);
			this.btnAltSetup.Name = "btnAltSetup";
			this.btnAltSetup.Size = new System.Drawing.Size(87, 22);
			this.btnAltSetup.TabIndex = 3;
			this.btnAltSetup.Text = "Alternate setup";
			this.btnAltSetup.UseVisualStyleBackColor = false;
			this.btnAltSetup.Click += new System.EventHandler(this.btnAltSetup_Click);
			//
			// pnlAltPopup
			//
			this.pnlAltPopup.BackColor = System.Drawing.Color.Transparent;
			this.pnlAltPopup.Controls.Add(this.btnAltSetup);
			this.pnlAltPopup.Location = new System.Drawing.Point(290, 223);
			this.pnlAltPopup.Name = "pnlAltPopup";
			this.pnlAltPopup.Size = new System.Drawing.Size(87, 22);
			this.pnlAltPopup.TabIndex = 5;
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(3, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(41, 18);
			this.label3.TabIndex = 7;
			this.label3.Text = "Web:";
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(3, 18);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(35, 13);
			this.label4.TabIndex = 8;
			this.label4.Text = "Email:";
			//
			// label5
			//
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(3, 36);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(41, 13);
			this.label5.TabIndex = 9;
			this.label5.Text = "Phone:";
			//
			// lblInstName
			//
			this.lblInstName.AutoSize = true;
			this.lblInstName.Location = new System.Drawing.Point(3, 2);
			this.lblInstName.MaximumSize = new System.Drawing.Size(375, 0);
			this.lblInstName.Name = "lblInstName";
			this.lblInstName.Size = new System.Drawing.Size(80, 13);
			this.lblInstName.TabIndex = 6;
			this.lblInstName.Text = "institution name";
			//
			// lblWeb
			//
			this.lblWeb.AutoEllipsis = true;
			this.lblWeb.AutoSize = true;
			this.lblWeb.Location = new System.Drawing.Point(50, 0);
			this.lblWeb.MaximumSize = new System.Drawing.Size(250, 0);
			this.lblWeb.MinimumSize = new System.Drawing.Size(0, 18);
			this.lblWeb.Name = "lblWeb";
			this.lblWeb.Size = new System.Drawing.Size(137, 18);
			this.lblWeb.TabIndex = 7;
			this.lblWeb.Text = "https://www.institution.com";
			//
			// lblEmail
			//
			this.lblEmail.AutoSize = true;
			this.lblEmail.Location = new System.Drawing.Point(50, 18);
			this.lblEmail.MinimumSize = new System.Drawing.Size(0, 18);
			this.lblEmail.Name = "lblEmail";
			this.lblEmail.Size = new System.Drawing.Size(109, 18);
			this.lblEmail.TabIndex = 8;
			this.lblEmail.Text = "email@institution.com";
			//
			// lblPhone
			//
			this.lblPhone.AutoSize = true;
			this.lblPhone.Location = new System.Drawing.Point(50, 36);
			this.lblPhone.MinimumSize = new System.Drawing.Size(0, 18);
			this.lblPhone.Name = "lblPhone";
			this.lblPhone.Size = new System.Drawing.Size(79, 18);
			this.lblPhone.TabIndex = 9;
			this.lblPhone.Text = "003245678743";
			//
			// tblContactInfo
			//
			this.tblContactInfo.AutoSize = true;
			this.tblContactInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tblContactInfo.ColumnCount = 2;
			this.tblContactInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.61539F));
			this.tblContactInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80.38461F));
			this.tblContactInfo.Controls.Add(this.label3, 0, 0);
			this.tblContactInfo.Controls.Add(this.lblPhone, 1, 2);
			this.tblContactInfo.Controls.Add(this.label5, 0, 2);
			this.tblContactInfo.Controls.Add(this.lblEmail, 1, 1);
			this.tblContactInfo.Controls.Add(this.label4, 0, 1);
			this.tblContactInfo.Controls.Add(this.lblWeb, 1, 0);
			this.tblContactInfo.Location = new System.Drawing.Point(7, 19);
			this.tblContactInfo.Name = "tblContactInfo";
			this.tblContactInfo.RowCount = 3;
			this.tblContactInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContactInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContactInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContactInfo.Size = new System.Drawing.Size(240, 54);
			this.tblContactInfo.TabIndex = 10;
			//
			// groupBox1
			//
			this.groupBox1.AutoSize = true;
			this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox1.Controls.Add(this.tblContactInfo);
			this.groupBox1.Location = new System.Drawing.Point(5, 35);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(253, 92);
			this.groupBox1.TabIndex = 11;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Contact info";
			//
			// linkLabel1
			//
			this.linkLabel1.Location = new System.Drawing.Point(179, 149);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(117, 23);
			this.linkLabel1.TabIndex = 12;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "linkLabel1";
			//
			// frmSelfExtract
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(378, 246);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.lblInstName);
			this.Controls.Add(this.pnlAltPopup);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "frmSelfExtract";
			this.Text = "frm1";
			this.Load += new System.EventHandler(this.frmSelfExtract_Load);
			this.pnlAltPopup.ResumeLayout(false);
			this.tblContactInfo.ResumeLayout(false);
			this.tblContactInfo.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnAltSetup;
		private System.Windows.Forms.Panel pnlAltPopup;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lblInstName;
		private System.Windows.Forms.Label lblWeb;
		private System.Windows.Forms.Label lblEmail;
		private System.Windows.Forms.Label lblPhone;
		private System.Windows.Forms.TableLayoutPanel tblContactInfo;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.LinkLabel linkLabel1;
	}
}