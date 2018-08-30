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
			// gets redirect link from parent form label, converts to lower case
			string redirectString = parentForm.RedirectUrl.ToLower();
			// sets text of label
			lblRedirectLink.Text = redirectString;
			// checks if link starts with an accepted prefix
			if (redirectString.StartsWith("http://") || redirectString.StartsWith("https://") ||
				redirectString.StartsWith("www."))
			{
				// sets linkdata
				var redirectLink = new LinkLabel.Link { LinkData = lblRedirectLink.Text };
				lblRedirectLink.Links.Add(redirectLink);
			}
			// disables link, but still displays it
			else
			{
				lblRedirectLink.Enabled = false;
			}

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
