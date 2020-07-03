using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using EduroamConfigure;
using System.ComponentModel;

namespace EduroamApp
{
    /// <summary>
    /// Lets the user select a country, institution and optionally a profile, and downloads an appropriate EAP-config file.
    /// </summary>
    public partial class frmSelectInstitution : Form
    {
        private readonly frmParent frmParent; // makes parent form accessible from this class
        private IdentityProviderDownloader Downloader { get => frmParent.IdpDownloader; }
        private List<IdentityProvider> identityProviders = new List<IdentityProvider>(); // list containing all identity providers
        private List<IdentityProvider> allIdentityProviders; // TODO: delete?
        public int idProviderId; // id of selected institution
        private BackgroundWorker _worker;

       

        public frmSelectInstitution(frmParent parentInstance)
        {
            // gets parent form instance
            frmParent = parentInstance;
            InitializeComponent();
            InitWorker();

        }

        /// <summary>
        /// Called fron InitializeComponents(). Used to get various components ready
        /// </summary>
        private void frmSelectInstitution_Load(object sender, EventArgs e)
        {
            frmParent.BtnNextEnabled = false;

            tlpLoading.BringToFront();
            tlpLoading.Visible = true;

            tbSearch.Visible = true;
            tbSearch.Enabled = true;
            tbSearch.ReadOnly = true;
            tbSearch.BackColor = System.Drawing.SystemColors.Window;
            lbInstitution.Visible = true;
            lbInstitution.Enabled = true;


            // display Eduroam logo. Applicable when returning from the Summary form and
            // institution logo was previously set, deactivating eduroam logo
            frmParent.WebEduroamLogo.Visible = true;
            frmParent.RedirectUrl = "";

            // make user autoselect search
            this.ActiveControl = tbSearch;
           
            tlpLoading.Visible = false;
        }

        private void InitWorker()
        {

            if (_worker != null)
            {
                _worker.Dispose();
            }

            _worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };

            _worker.DoWork += DoWork;
            _worker.RunWorkerCompleted += RunWorkerCompleted;
            _worker.RunWorkerAsync();
        }

        void DoWork(object sender, DoWorkEventArgs e)
        {
            PopulateInstitutions();
        }

        void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            tbSearch.Enabled = true;
            lbInstitution.Enabled = true; ;
        }

        public void StartLoading()
        {
            tbSearch.Visible = true;
            tbSearch.Enabled = true;
            lbInstitution.Visible = true;
            lbInstitution.Enabled = true; ;
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
        /// Called when the form is created to present the 10 closest providers
        /// </summary>
        private void PopulateInstitutions()
        {
            try
            {
                allIdentityProviders = Downloader.Providers;
                updateInstitutions(Downloader.GetClosestProviders(limit: 10));
            }
            catch (EduroamAppUserError e)
            {
                EduroamAppExceptionHandler(e);
            }
        }

        /// <summary>
        /// Used to update institution list portrayed to users.
        /// Called by different thread than Winform thread
        /// </summary>
        private void updateInstitutions(List<IdentityProvider> institutions)
        {
            BeginInvoke(new Action(() =>
            {
                lbInstitution.Items.Clear();

                identityProviders = institutions;

                lbInstitution.Items.AddRange(identityProviders.Select(provider => provider.Name).ToArray());

                tbSearch.ReadOnly = false;
            }));
        }

        /// <summary>
        /// Called when user types something in the seach bar
        /// </summary>
        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            updateInstitutions(
                IdentityProviderParser.SortByQuery(
                    allIdentityProviders, 
                    tbSearch.Text, 
                    limit: 100));
        }


        private void lbInstitution_SelectedIndexChanged(object sender, EventArgs e)
        {
            // if user clicks on empty area of the listbox it will cause event but no item is selected
            if (lbInstitution.SelectedItem == null) return;
            // select provider ID based on chosen profile name
            idProviderId = identityProviders
                .Where(x => x.Name == (string)lbInstitution.SelectedItem)
                .Select(x => x.cat_idp)
                .First();

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
            MessageBox.Show(ex.UserFacingMessage,
                "eduroam - Web exception",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


    }
}
