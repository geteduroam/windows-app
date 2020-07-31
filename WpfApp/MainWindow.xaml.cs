using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using WpfApp.Menu;
using EduroamConfigure;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum FormId
        {
            MainMenu,
            SelectInstitution,
            SelectProfile,
            ProfileOverview,
            Download,
            Login,
            Connect,
            Redirect,
            SaveAndQuit,
            Loading
        }
        private readonly List<FormId> historyFormId = new List<FormId>();
        private FormId currentFormId;
        private MainMenu pageMainMenu;
        private SelectInstitution pageSelectInstitution;
        private SelectProfile pageSelectProfile;
        private ProfileOverview pageProfileOverview;
        private Loading pageLoading;
        private bool Online;
        private EapConfig eapConfig;
        public ProfileStatus ProfileCondition { get; set; }
        public IdentityProviderDownloader IdpDownloader;

        public enum ProfileStatus
        {
            NoneConfigured,
            Incomplete,
            Working,
        }
        public EapConfig.AuthenticationMethod AuthMethod;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                IdpDownloader = new IdentityProviderDownloader();
                Online = true;
            }
            catch (ApiException)
            {
                Online = false;
            }

            LoadPageMainMenu();
        }

        public void Navigate(Page nextPage)
        {
            Main.Content = nextPage;
        }
        
        public void NextPage()
        {
            // adds current form to history for easy backtracking
            historyFormId.Add(currentFormId);
            switch (currentFormId)
            {
                case FormId.MainMenu:
                    LoadPageSelectInstitution();
                    break;

                case FormId.SelectInstitution:
                    var profiles = GetProfiles((int)pageSelectInstitution.IdProviderId);
                    if (profiles.Count == 1)
                    {
                        string autoProfileId = profiles.FirstOrDefault().Id;
                        if (!string.IsNullOrEmpty(autoProfileId))
                        {
                            HandleProfileSelect(autoProfileId);
                            break;
                        }
                    }
                    LoadPageSelectProfile();
                    break;

                case FormId.SelectProfile:
                    string profileId = pageSelectProfile.ProfileId;
                    HandleProfileSelect(profileId);
                    break;
            }

            
            // removes current form from history if it gets added twice
            if (historyFormId.LastOrDefault() == currentFormId) historyFormId.RemoveAt(historyFormId.Count - 1);
      
        }

        public void PreviousPage()
        {
            // clears logo if going back from summary page
            //if (currentFormId == FormId.Summary) ResetLogo();

            switch (historyFormId.Last())
            {
                case FormId.MainMenu:
                    LoadPageMainMenu();
                    break;

                case FormId.SelectInstitution:
                    LoadPageSelectInstitution();
                    break;
            }

            // removes current form from history
            historyFormId.RemoveAt(historyFormId.Count - 1);
        }

        // downloads eap config based on profileId
        // seperated into its own function as this can happen either through
        // user selecting a profile or a profile being autoselected
        private async void HandleProfileSelect(string profileId)
        {
            LoadPageLoading();
            IdentityProviderProfile profile = IdpDownloader.GetProfileFromId(profileId);
            eapConfig = await DownloadEapConfig(profile);

            if (eapConfig != null)
            {
                LoadPageProfileOverview(eapConfig);
            }
            else if (!string.IsNullOrEmpty(profile.redirect))
            {
                // TODO: add option to go to selectmethod from redirect
                LoadPageRedirect();
            }
        }

        /// <summary>
        /// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
        /// </summary>
        private List<IdentityProviderProfile> GetProfiles(int providerId)
        {
            return IdpDownloader.GetIdentityProviderProfiles(providerId);

        }

        /// <summary>
        /// Gets EAP-config file, either directly or after browser authentication.
        /// Prepares for redirect if no EAP-config.
        /// </summary>
        /// <returns>EapConfig object.</returns>
        public async Task<EapConfig> DownloadEapConfig(IdentityProviderProfile profile)
        {
            // eap config file as string
            string eapXmlString;

            // if OAuth
            if (profile.oauth)
            {
                // get eap config file from browser authenticate
                try
                {
                    OAuth oauth = new OAuth(profile.authorization_endpoint, profile.token_endpoint, profile.eapconfig_endpoint);
                    // generate authURI based on redirect
                    string authUri = oauth.GetAuthUri();
                    // get local listening uri prefix
                    string prefix = oauth.GetRedirectUri();
                    // browser authenticate
                    string responseUrl = GetResponseUrl(prefix, authUri);
                    // get eap-config string if available
                    eapXmlString = oauth.GetEapConfigString(responseUrl);
                }
                catch (EduroamAppUserError ex)
                {
                    MessageBox.Show(ex.UserFacingMessage);
                    eapXmlString = "";
                }
                // return focus to application
                Activate();
            }            
            else
            {
                eapXmlString = await Task.Run(() => IdpDownloader.GetEapConfigString(profile.Id));
            }

            // if not empty, creates and returns EapConfig object from Eap string

            if (string.IsNullOrEmpty(eapXmlString))
            {
                return null;
            }

            try
            {
                // if not empty, creates and returns EapConfig object from Eap string
                return EapConfig.FromXmlData(uid: profile.Id, eapXmlString);
            }
            catch (XmlException ex)
            {
                MessageBox.Show(
                    "The selected institution or profile is not supported. " +
                    "Please select a different institution or profile.\n" +
                    "Exception: " + ex.Message);
                return null;
            }
        }


        // TODO: make new responesurl thing to receive 

        /// <summary>
        /// Gets a response URL after doing Browser authentication with Oauth authUri.
        /// </summary>
        /// <returns>response Url as string.</returns>
        public string GetResponseUrl(string redirectUri, string authUri)
        {
            /*
            using var waitForm = new frmWaitDialog(redirectUri, authUri);
            DialogResult result = waitForm.ShowDialog();
            if (result != DialogResult.OK)
            {
                return "";
            }
            return waitForm.responseUrl;  //= WebServer.NonblockingListener(redirectUri, authUri, parentLocation);
            */
            return "";
        }

        public void LoadPageMainMenu()
        {
            currentFormId = FormId.MainMenu;
            pageMainMenu = new MainMenu(this);
            Navigate(pageMainMenu);
        }

        public void LoadPageSelectInstitution()
        {
            currentFormId = FormId.SelectInstitution;
            pageSelectInstitution = new SelectInstitution(this);
            Navigate(pageSelectInstitution);
        }

        public void LoadPageSelectProfile()
        {
            currentFormId = FormId.SelectProfile;
            pageSelectProfile = new SelectProfile(this, pageSelectInstitution.IdProviderId);
            Navigate(pageSelectProfile);
        }

        public void LoadPageProfileOverview(EapConfig eapConfig)
        {
            currentFormId = FormId.SelectProfile;
            pageProfileOverview = new ProfileOverview(this, eapConfig);
            Navigate(pageProfileOverview);
        }

        public void LoadPageRedirect()
        {
            currentFormId = FormId.Redirect;
        }

        public void LoadPageLoading()
        {
            currentFormId = FormId.Loading;
            pageLoading = new Loading();
            Navigate(pageLoading);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            NextPage();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            PreviousPage();
        }
    }
}
