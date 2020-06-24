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
    public partial class frmDownload : Form
    {
        private readonly frmParent frmParent; // makes parent form accessible from this class
        private List<IdentityProvider> identityProviders = new List<IdentityProvider>(); // list containing all identity providers
        private List<IdentityProvider> allIdentityProviders;
        private List<IdentityProviderProfile> idProviderProfiles; // list containing all profiles of an identity provider
        private int idProviderId; // id of selected institution
        public string ProfileId { get; set; } // id of selected institution profile

        public frmDownload(frmParent parentInstance)
        {
            // gets parent form instance
            frmParent = parentInstance;
            InitializeComponent();
        }

        private async void frmDownload_Load(object sender, EventArgs e)
        {
            // hides certain controls while loading
            //HideControls();
            // displays loading animation while fetching list of institutions
            tlpLoading.BringToFront();
            tlpLoading.Visible = true;
            // resets redirect url
            frmParent.RedirectUrl = "";

            lblSelectProfile.Visible = true;
            lblSelectProfile.Enabled = false;
            tbSearch.Enabled = false;

            cboProfiles.Visible = true;
            cboProfiles.Enabled = false;

            //HideControls();

            // async method to get list of institutions
            bool getInstSuccess = await Task.Run(() => GetAllInstitutions());

            if (getInstSuccess)
            {
                // enables controls
                tlpLoading.Visible = false;
                frmParent.BtnNextEnabled = true;

                PopulateInstitutions();
                cboProfiles.Visible = true;
                cboProfiles.Enabled = false;

                lblSearch.Visible = true;
                tbSearch.Visible = true;
                tbSearch.Enabled = true;
                lbInstitution.Visible = true;

                lblSelectProfile.Visible = true;
                lblSelectProfile.Enabled = false;
                this.ActiveControl = tbSearch;
            }
            else
            {
                tlpLoading.Visible = false;
                lblError.Visible = true;
            }
        }

        /// <summary>
        /// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
        /// </summary>
        private bool GetAllInstitutions()
        {
            try
            {
                allIdentityProviders = IdentityProviderDownloader.GetAllIdProviders();
                return true;
            }
            catch (EduroamAppUserError ex)
            {
                lblError.Text = ex.UserFacingMessage;
            }
            return false;
        }


        private void PopulateInstitutions()
        {
            List<IdentityProvider> closeProviders = IdentityProviderDownloader.GetClosestProviders(10, frmParent.GeoWatcher.Position.Location);
            updateInstitutions(closeProviders);
        }


        private void updateInstitutions(List<IdentityProvider> institutions)
        {
            lbInstitution.Items.Clear();

            identityProviders = institutions;

            lbInstitution.Items.AddRange(identityProviders.Select(provider => provider.Name).ToArray());
        }


        // gets profile id of selected profile
        private void cboProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboProfiles.Text != "")
            {
                // gets profile id of profile selected in combobox
                ProfileId = idProviderProfiles.Where(profile => profile.Name == cboProfiles.Text).Select(x => x.Id).Single();

            }
        }

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            List<IdentityProvider> sortedProviders = IdentityProviderParser.SortBySearch(allIdentityProviders, tbSearch.Text);
            updateInstitutions(sortedProviders);
        }

        private void lbInstitution_SelectedIndexChanged(object sender, EventArgs e)
        {
            // clear combobox
            cboProfiles.Items.Clear();
            // clear selected profile
            ProfileId = null;

            // gets id of institution selected in combobox
            idProviderId = identityProviders.Where(x => x.Name == lbInstitution.Text).Select(x => x.cat_idp).First();

            // get IdentityProviderProfile object for provider, containing all profiles
            try
            {
                idProviderProfiles = IdentityProviderDownloader.GetIdentityProviderProfiles(idProviderId);
            }
            catch (EduroamAppUserError ex)
            {
                EduroamAppExceptionHandler(ex);
                return;
            }

            // add profiles to combobox
            cboProfiles.Items.AddRange(idProviderProfiles.Select(profile => profile.Name).ToArray());

            // if an identity provider has more than one, activate combobox so a different profile can be used
            if (idProviderProfiles.Count > 1)
            {
                // enable combobox
                cboProfiles.Enabled = true;

                // enable label
                lblSelectProfile.Enabled = true;
            }
            else
            {
                // get first profile from combobox list
                IdentityProviderProfile profile = IdentityProviderDownloader.GetProfileFromId(idProviderProfiles.First().Id);
                // set first profile to be selected automatically
                cboProfiles.SelectedIndex = cboProfiles.FindStringExact(profile.Name);
                // disable combobox
                cboProfiles.Enabled = false;

                // disble label
                lblSelectProfile.Enabled = false;
            }
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
            lblSelectProfile.Visible = false;
            lblSearch.Visible = false;
            tbSearch.Visible = false;
            lbInstitution.Visible = false;
            cboProfiles.Visible = false;
            lblSelectProfile.Visible = false;
            frmParent.BtnNextEnabled = false;


        }

        private void lblSearch_Click(object sender, EventArgs e)
        {

        }
    }
}
