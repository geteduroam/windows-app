namespace EduroamApp
{
	partial class frm1
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
			this.label2 = new System.Windows.Forms.Label();
			this.btnAltSetup = new System.Windows.Forms.Button();
			this.pnlAltPopup = new System.Windows.Forms.Panel();
			this.pnlAltPopup.SuspendLayout();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(54, 131);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(122, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Press Install to continue.";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(53, 50);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(119, 20);
			this.label2.TabIndex = 2;
			this.label2.Text = "eduroam Setup";
			//
			// btnAltSetup
			//
			this.btnAltSetup.BackColor = System.Drawing.SystemColors.ControlLight;
			this.btnAltSetup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnAltSetup.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.btnAltSetup.FlatAppearance.BorderSize = 2;
			this.btnAltSetup.Location = new System.Drawing.Point(0, 0);
			this.btnAltSetup.Name = "btnAltSetup";
			this.btnAltSetup.Size = new System.Drawing.Size(115, 26);
			this.btnAltSetup.TabIndex = 3;
			this.btnAltSetup.Text = "Alternate setup";
			this.btnAltSetup.UseVisualStyleBackColor = false;
			this.btnAltSetup.Click += new System.EventHandler(this.btnAltSetup_Click);
			//
			// pnlAltPopup
			//
			this.pnlAltPopup.BackColor = System.Drawing.Color.Transparent;
			this.pnlAltPopup.Controls.Add(this.btnAltSetup);
			this.pnlAltPopup.Location = new System.Drawing.Point(321, 328);
			this.pnlAltPopup.Name = "pnlAltPopup";
			this.pnlAltPopup.Size = new System.Drawing.Size(115, 26);
			this.pnlAltPopup.TabIndex = 5;
			//
			// frm1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(448, 366);
			this.Controls.Add(this.pnlAltPopup);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "frm1";
			this.Text = "frm1";
			this.pnlAltPopup.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnAltSetup;
		private System.Windows.Forms.Panel pnlAltPopup;
	}
}