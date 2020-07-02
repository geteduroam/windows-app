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
    public partial class frmSelectInstitution : Form
    {
        private readonly frmParent frmParent; // makes parent form accessible from this class
        private List<IdentityProvider> identityProviders = new List<IdentityProvider>(); // list containing all identity providers
        private List<IdentityProvider> allIdentityProviders;
        public int idProviderId; // id of selected institution
       // private static string helpString = "Search here ..";

       

        public frmSelectInstitution(frmParent parentInstance)
        {
            // gets parent form instance
            frmParent = parentInstance;
            InitializeComponent();

        }

        /// <summary>
        /// Called fron InitializeComponents(). Used to get various components ready
        /// </summary>
        private async void frmDownload_Load(object sender, EventArgs e)
        {
            StartLoading();
            frmParent.WebEduroamLogo.Visible = true;
            frmParent.RedirectUrl = "";
            lbInstitution.Enabled = false;
            frmParent.BtnNextEnabled = false;

            // async method to get list of institutions
            bool getInstSuccess = await Task.Run(() => GetAllInstitutions());

            if (getInstSuccess)
            {
                PopulateInstitutions();
                this.ActiveControl = tbSearch;
            }
            else
            {
                lblError.Visible = true;
            }

            StopLoading();
        }

        public void StartLoading()
        {
            tbSearch.Visible = false;
            tbSearch.Enabled = false;
            lbInstitution.Visible = false;
            lbInstitution.Enabled = false;
            tlpLoading.BringToFront();
            tlpLoading.Visible = true;
        }

        private void StopLoading()
        {
            tbSearch.Visible = true;
            tbSearch.Enabled = true;
            lbInstitution.Visible = true;
            lbInstitution.Enabled = true;
            tlpLoading.Visible = false;
        }

        /// <summary>
        /// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
        /// </summary>
        private bool GetAllInstitutions()
        {
            try
            {
                //allIdentityProviders = IdentityProviderDownloader.GetAllIdProviders();
                allIdentityProviders = frmParent.Providers;
                return true;
            }
            catch (EduroamAppUserError ex)
            {
                lblError.Text = ex.UserFacingMessage;
            }
            return false;
        }

        /// <summary>
        /// Called when the form is created to present the 10 closest providers
        /// </summary>
        private void PopulateInstitutions()
        {
            try
            {
                List<IdentityProvider> closeProviders = IdentityProviderDownloader.GetClosestProviders(10, frmParent.GeoWatcher.Position.Location);
                updateInstitutions(closeProviders);
            }
            catch (EduroamAppUserError e)
            {
                EduroamAppExceptionHandler(e);
            }
        }

        /// <summary>
        /// Used to update institution list portrayed to users
        /// </summary>
        private void updateInstitutions(List<IdentityProvider> institutions)
        {
            lbInstitution.Items.Clear();

            identityProviders = institutions;

            lbInstitution.Items.AddRange(identityProviders.Select(provider => provider.Name).ToArray());
        }

        /// <summary>
        /// Called when user types something in the seach bar
        /// </summary>
        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            List<IdentityProvider> sortedProviders = IdentityProviderParser.SortBySearch(allIdentityProviders, tbSearch.Text);
            updateInstitutions(sortedProviders);
        }


        private void lbInstitution_SelectedIndexChanged(object sender, EventArgs e)
        {
            // if user clicks on empty area of the listbox it will cause event but no item is selected
            if (lbInstitution.SelectedItem == null) return;
            // select provider ID based on chosen profile name
            idProviderId = identityProviders.Where(x => x.Name == (string) lbInstitution.SelectedItem).Select(x => x.cat_idp).First();
            // update parent state. frmParent.idProviderId is used to create frmSelectProfile
            //frmParent.idProviderId = idProviderId;
            frmParent.BtnNextEnabled = true;

        }

        private void lbInstitution_DoubleClick(object sender, EventArgs e)
        {
            frmParent.btnNext_Click(sender, e);
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
            MessageBox.Show(ex.UserFacingMessage,
                   "eduroam - Web exception",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        /// <summary>
        /// Hides all controls on form.
        /// </summary>
        private void HideControls()
        {
            lblError.Visible = false;
            tbSearch.Visible = false;
            lbInstitution.Visible = false;
            frmParent.BtnNextEnabled = false;


        }

    }
}
