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

        // TODO: Make Title contain more words / wrap around
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
            Loading,
            InstallCertificates,
        }

        public enum ProfileStatus
        {
            NoneConfigured,
            Incomplete,
            Working,
        }

        private readonly List<FormId> historyFormId = new List<FormId>();
        private FormId currentFormId;
        private MainMenu pageMainMenu;
        private SelectInstitution pageSelectInstitution;
        private SelectProfile pageSelectProfile;
        private ProfileOverview pageProfileOverview;
        private Loading pageLoading;
        private Login pageLogin;
        private CertificateOverview pageCertificateOverview;
        private bool Online;
        private EapConfig eapConfig;
        public ProfileStatus ProfileCondition { get; set; }
        public IdentityProviderDownloader IdpDownloader;
        public bool EduroamAvailable { get; set; }
        public EapConfig.AuthenticationMethod AuthMethod { get; set; }
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
        
        public async void NextPage()
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
                            // if profile could not be handled then return to form
                            if(! await HandleProfileSelect(autoProfileId)) LoadPageSelectInstitution(refresh: false);
                            break;
                        }
                    }
                    LoadPageSelectProfile();
                    break;

                case FormId.SelectProfile:
                    string profileId = pageSelectProfile.ProfileId;
                    // if profile could not be handled then return to form
                    if (!await HandleProfileSelect(profileId)) LoadPageSelectProfile(refresh: false);
                    break;
               // case FormId.ProfileOverview:
               //     LoadPageInstallCertificates();
                case FormId.ProfileOverview:
                    LoadPageCertificateOverview();
                    break;
                case FormId.InstallCertificates:
                    LoadPageLogin();
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
                case FormId.SelectProfile:
                    LoadPageSelectProfile();
                    break;
                case FormId.ProfileOverview:
                    LoadPageProfileOverview(eapConfig);
                    break;
            }

            // removes current form from history
            historyFormId.RemoveAt(historyFormId.Count - 1);
        }

        // downloads eap config based on profileId
        // seperated into its own function as this can happen either through
        // user selecting a profile or a profile being autoselected
        private async Task<bool> HandleProfileSelect(string profileId)
        {
            LoadPageLoading();           
            IdentityProviderProfile profile = IdpDownloader.GetProfileFromId(profileId);
            try
            {
                eapConfig = await DownloadEapConfig(profile);
            }
            catch (EduroamAppUserError ex) // TODO: register this in some higher level
            {
                MessageBox.Show(
                    ex.UserFacingMessage,
                    "eduroam - Exception");
                eapConfig = null;
                
            }

            // reenable buttons after LoadPageLoading() disables them
            btnBack.IsEnabled = true;
            btnNext.IsEnabled = true;

            if (eapConfig != null)
            {
                LoadPageProfileOverview(eapConfig);
                return true;
            }
            else if (!string.IsNullOrEmpty(profile.redirect))
            {
                // TODO: add option to go to selectmethod from redirect
                LoadPageRedirect();
                return true;
            }
            return false;
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

            if (string.IsNullOrEmpty(profile.Id))
            {
                return null;
            };

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
            else if (!String.IsNullOrEmpty(profile.redirect))
            {
                //TODO handle redirect
                // makes redirect link accessible in parent form
                //RedirectUrl = redirect;
                return null;
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

        public void LoadPageMainMenu(bool refresh = true)
        {
            currentFormId = FormId.MainMenu;
            //lblTitle.Content = "Connect to Eduroam";
            btnNext.Visibility = Visibility.Hidden;
            btnBack.Visibility = Visibility.Hidden;
            //lblTitle.Visibility = Visibility.Hidden;
            if (refresh) pageMainMenu = new MainMenu(this);
            Navigate(pageMainMenu);
        }

        public void LoadPageSelectInstitution(bool refresh = true)
        {
            currentFormId = FormId.SelectInstitution;
            //lblTitle.Content = "Select your institution";
            //lblTitle.Visibility = Visibility.Visible;        
            btnNext.Visibility = Visibility.Visible;
            btnNext.Content = "Next >";
            btnBack.IsEnabled = true;
            btnBack.Visibility = Visibility.Visible;
            if (refresh) pageSelectInstitution = new SelectInstitution(this);

            Navigate(pageSelectInstitution);
        }

        public void LoadPageSelectProfile(bool refresh = true)
        {
            currentFormId = FormId.SelectProfile;
            //lblTitle.Content = "Select Profile";
            btnNext.Visibility = Visibility.Visible;
            if (refresh) pageSelectProfile = new SelectProfile(this, pageSelectInstitution.IdProviderId);
            Navigate(pageSelectProfile);
        }

        public void LoadPageProfileOverview(EapConfig eapConfig, bool refresh = true)
        {
            currentFormId = FormId.ProfileOverview;
            //lblTitle.Content = eapConfig.InstitutionInfo.DisplayName;
            btnNext.Visibility = Visibility.Visible;
            btnBack.Visibility = Visibility.Visible;
            btnNext.Content = eapConfig.AuthenticationMethods.First().EapType == EapType.TLS ? "Connect" : "Next";
            if (refresh) pageProfileOverview = new ProfileOverview(this, eapConfig);
            Navigate(pageProfileOverview);
        }

        public void LoadPageCertificateOverview(bool refresh = true)
        {
            currentFormId = FormId.InstallCertificates;
            //lblTitle.Content = "";
            btnBack.Visibility = Visibility.Visible;
            btnBack.IsEnabled = true;
            if (refresh) pageCertificateOverview = new CertificateOverview(this, eapConfig);
            Navigate(pageCertificateOverview);
        }

        public void LoadPageLogin(bool refresh = true)
        {
            currentFormId = FormId.Login;
            if (refresh) pageLogin = new Login(this);
            Navigate(pageLogin);
        }

        public void LoadPageRedirect(bool refresh = true)
        {
            currentFormId = FormId.Redirect;
        }

        public void LoadPageLoading(bool refresh = true)
        {
            currentFormId = FormId.Loading;
            //lblTitle.Content = "Loading ...";
            btnBack.IsEnabled = false;
            btnNext.IsEnabled = false;
            if (refresh) pageLoading = new Loading(this);
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
