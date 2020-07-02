using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using EduroamConfigure;

namespace EduroamApp
{
    /// <summary>
    /// Lets the user select a country, institution and optionally a profile, and downloads an appropriate EAP-config file.
    /// </summary>
    public partial class frmSelectProfile : Form
    {
        private readonly frmParent frmParent; // makes parent form accessible from this class
        private List<IdentityProviderProfile> idProviderProfiles; // list containing all profiles of an identity provider
        private int idProviderId; // id of selected institution
        //private static string helpString = "Search here ..";
        public string ProfileId { get; set; } // id of selected institution profile

       

        public frmSelectProfile(frmParent parentInstance, int providerId)
        {
            // gets parent form instance
            frmParent = parentInstance;
            idProviderId = providerId;
            InitializeComponent();
        }

        private async void frmSelectProfile_Load(object sender, EventArgs e)
        {
            this.Hide();
            frmParent.WebEduroamLogo.Visible = true;
            frmParent.BtnNextEnabled = false;

            frmParent.RedirectUrl = "";
            lbProfile.Enabled = false;

            //async method to get list of institutions
            bool getInstSuccess = await Task.Run(() => GetProfiles());

            if (getInstSuccess)
            {

                PopulateProfiles();
                lbProfile.Enabled = true;

                // autoselect first profile
                lbProfile.SetSelected(0, true);

            }
            else
            {

            }

            this.Show();

        }

        // double clicking profile acts as clicking "next"
        private void lbProfile_DoubleClick(object sender, EventArgs e)
        {
            frmParent.btnNext_Click(sender, e);
        }

        /// <summary>
        /// Shows all profles in a textbox so the user can choose one
        /// </summary>
        private void PopulateProfiles()
        {
            lbProfile.Items.Clear();

            lbProfile.Items.AddRange(idProviderProfiles.Select(provider => provider.Name).ToArray());
        }

        /// <summary>
        /// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
        /// </summary>
        private bool GetProfiles()
        {
            try
            {
                idProviderProfiles = frmParent.Downloader.GetIdentityProviderProfiles(idProviderId);
                return true;
            }
            catch (EduroamAppUserError ex)
            {
                lblError.Text = ex.UserFacingMessage;
            }
            return false;
        }




        /// <summary>
        /// Called when user selects a profile
        /// </summary>
        private void lbProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            // if user clicks on empty area of the listbox it will cause event but no item is selected
            if (lbProfile.SelectedItem == null) return;

            // gets id of selected profile
            ProfileId = idProviderProfiles.Where(x => x.Name == (string) lbProfile.SelectedItem).Select(x => x.Id).Single();
            frmParent.BtnNextEnabled = true;
        }


        /// <summary>
        /// Handles EduroamApp exxceptions.
        /// </summary>
        /// <param name="ex">WebException.</param>
        private void EduroamAppExceptionHandler(EduroamAppUserError ex)
        {
            //HideControls();
            //lblError.Text = ex.UserFacingMessage;
            //lblError.Visible = true;
            MessageBox.Show(ex.UserFacingMessage, "eduroam - Web exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        /// <summary>
        /// Hides all controls on form.
        /// </summary>
        private void HideControls()
        {
            lblError.Visible = false;
            lbProfile.Visible = false;
            frmParent.BtnNextEnabled = false;
        }

    }
}
