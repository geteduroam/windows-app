using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace EduroamApp
{
	public partial class frmRedirect : Form
	{
		private readonly frmParent parentForm;

		public frmRedirect(frmParent parentInstance)
		{
			parentForm = parentInstance;
			InitializeComponent();
		}

		private void frmRedirect_Load(object sender, EventArgs e)
		{
			// sets text of label
			lblRedirectLink.Text = parentForm.LblRedirect;
			// sets linkdata
			var redirectLink = new LinkLabel.Link {LinkData = lblRedirectLink.Text};
			lblRedirectLink.Links.Add(redirectLink);
		}

		private void lblRedirectLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// opens redirect link in browser
			Process.Start(e.Link.LinkData as string);
			// closes application
			parentForm.Close();
		}
	}
}
