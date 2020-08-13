using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using Hardcodet.Wpf.TaskbarNotification;
using EduroamConfigure;
using WpfApp.Menu;
using Hardcodet.Wpf.TaskbarNotification.Interop;
using System.Windows.Navigation;

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
            CertificateOverview,
            TermsOfUse,
            OAuthWait,
            InstalledProfile,
        }

        public enum ProfileStatus
        {
            NoneConfigured,
            Configured,
            TestedWorking,
        }

        private readonly List<FormId> historyFormId = new List<FormId>();
        private FormId currentFormId;
        private MainMenu pageMainMenu;
        private SelectInstitution pageSelectInstitution;
        private SelectProfile pageSelectProfile;
        private ProfileOverview pageProfileOverview;
        private Loading pageLoading;
        private Login pageLogin;
        private TermsOfUse pageTermsOfUse;
        private CertificateOverview pageCertificateOverview;
        private Redirect pageRedirect;
        private OAuthWait pageOAuthWait;
        private InstalledProfile pageInstalledProfile;
        // this contains the 'active' eapConfig that is being used
        private EapConfig eapConfig;
        // If theres is a bundled config file then it is stored in this variable
        public EapConfig ExtractedEapConfig { get; set; }
        //ExtractFlag decides if the "Not affiliated with this institution? choose another one" text and button shows up on ProfileOverview or not
        public bool ExtractFlag { get; set; }
        public string PresetUsername { get; private set; }

        public ProfileStatus ProfileCondition { get; set; } // TODO: use this to determine if we need to clean up after a failed setup, together with SelfInstaller.IsInstalled
        public IdentityProviderDownloader IdpDownloader { get; private set; }
        public bool EduroamAvailable { get; set; }
        public MainWindow()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();

            if (!App.Installer.IsRunningInInstallLocation)
                TaskbarIcon.Visibility = Visibility.Hidden;

            if (App.StartHiddenInTray && App.Installer.IsRunningInInstallLocation)
                Hide();

            Load();
        }

        private void Load()
        {
            IdpDownloader = new IdentityProviderDownloader();
            ExtractedEapConfig = GetBundledEapConfig();
            if (PersistingStore.IdentityProvider != null)
            {
                LoadPageInstalledProfile();
            }
            else if (ExtractedEapConfig != null)
            {
                // loads summary form so user can confirm installation
                eapConfig = ExtractedEapConfig;
                ExtractFlag = true;
                LoadPageProfileOverview();
            }
            else
            {
                LoadPageMainMenu();
            }
        }

        public void Navigate(Page nextPage)
        {
            // if nothing to go back to, hide back button

            Main.Content = nextPage;
            ValidateBackButton();
        }

        public async void NextPage()
        {
            // adds current form to history for easy backtracking
            historyFormId.Add(currentFormId);
            switch (currentFormId)
            {
                case FormId.InstalledProfile:
                    if (pageInstalledProfile.GoToMain)
                    {
                        LoadPageMainMenu();
                    }
                    else if (pageInstalledProfile.ProfileId != null)
                    {
                        PresetUsername = pageInstalledProfile.ReinstallUsername;
                        await HandleProfileSelect(
                            pageInstalledProfile.ProfileId,
                            pageInstalledProfile.ReinstallEapConfigXml,
                            skipOverview: true);
                    }
                    else {
                        LoadPageMainMenu(); // sanity
                    }
                    break;
                case FormId.MainMenu:
                    if (pageMainMenu.LocalEapConfig != null)
                    {
                        eapConfig = pageMainMenu.LocalEapConfig;
                        LoadPageProfileOverview();
                        break;
                    }
                    if (pageMainMenu.UseExtracted)
                    {
                        eapConfig = ExtractedEapConfig;
                        LoadPageProfileOverview();
                        break;
                    }

                    LoadPageSelectInstitution();
                    break;

                case FormId.SelectInstitution:
                    var profiles = GetProfiles((int)pageSelectInstitution.IdProviderId);
                    if (profiles.Count == 1) // skip the profile select and go with the first one
                    {
                        string autoProfileId = profiles.FirstOrDefault().Id;
                        if (!string.IsNullOrEmpty(autoProfileId))
                        {
                            // if profile could not be handled then return to form
                            if (!await HandleProfileSelect(autoProfileId)) LoadPageSelectInstitution(refresh: false);
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

                case FormId.ProfileOverview:
                    if (pageProfileOverview.ShowTou)
                    {
                        LoadPageTermsOfUse();
                        break;
                    }
                    if (ConnectToEduroam.EnumerateCAInstallers(eapConfig)
                        .Any(installer => installer.IsInstalledByUs || !installer.IsInstalled))
                    {
                        LoadPageCertificateOverview();
                        break;
                    }

                    LoadPageLogin();
                    break;
                case FormId.TermsOfUse:
                    historyFormId.Remove(currentFormId);
                    PreviousPage();
                    break;

                case FormId.CertificateOverview:
                    LoadPageLogin();
                    break;

                case FormId.Login:
                    if (pageLogin.IsConnected)
                    {
                        if (!App.Installer.IsRunningInInstallLocation)
                        {
                            Shutdown();
                        }
                        else
                        {
                            Hide();
                            LoadPageInstalledProfile();
                            historyFormId.Clear();
                        }
                        break;
                    }
                    pageLogin.ConnectClick();
                    break;
                case FormId.Redirect:
                    break;
            }


            // removes current form from history if it gets added twice
            if (historyFormId.LastOrDefault() == currentFormId) historyFormId.RemoveAt(historyFormId.Count - 1);

            ValidateBackButton();

        }

        public void PreviousPage()
        {
            if (currentFormId == FormId.Login)
            {
                    pageLogin.IgnorePasswordChange = true;
                    pageLogin.dispatcherTimer.Stop();                
            }
            switch (historyFormId.Last())
            {
                case FormId.InstalledProfile:
                    LoadPageInstalledProfile();
                    break;
                case FormId.MainMenu:
                    LoadPageMainMenu();
                    break;
                case FormId.SelectInstitution:
                    if (currentFormId == FormId.OAuthWait) OAuthWait.CancelThread();
                    LoadPageSelectInstitution();
                    break;
                case FormId.SelectProfile:
                    if (currentFormId == FormId.OAuthWait) OAuthWait.CancelThread();
                    LoadPageSelectProfile();
                    break;
                case FormId.ProfileOverview:
                    LoadPageProfileOverview();
                    break;
                case FormId.CertificateOverview:
                    if (Main.Content == pageLogin)
                    {
                        pageLogin.IgnorePasswordChange = true;
                        pageLogin.dispatcherTimer.Stop();
                    }
                    LoadPageCertificateOverview();
                    break;
                case FormId.Login:
                    LoadPageLogin();
                    break;

            }

            // removes current form from history
            historyFormId.RemoveAt(historyFormId.Count - 1);

            ValidateBackButton();
        }
        /// <summary>
        /// Hide back button if theres no page to go back to
        /// </summary>
        private void ValidateBackButton() // TODO: rename this function
        {
            if (historyFormId.Count < 1)
                btnBack.Visibility = Visibility.Hidden;
        }

        public static bool CheckIfEapConfigIsSupported(EapConfig eapConfig)
        {
            if (!EduroamNetwork.EapConfigIsSupported(eapConfig))
            {
                MessageBox.Show(
                    "The profile you have selected is not supported by this application.",
                    "No supported authentification method was found.",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            return true;
        }

        /// <summary>
        /// downloads eap config based on profileId
        /// seperated into its own function as this can happen either through
        /// user selecting a profile or a profile being autoselected
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="eapConfigXml"></param>
        /// <param name="skipOverview"></param>
        /// <returns>True if function navigated somewhere</returns>
        private async Task<bool> HandleProfileSelect(string profileId, string eapConfigXml = null, bool skipOverview = false)
        {
            LoadPageLoading();
            IdentityProviderProfile profile = null;

            if (!string.IsNullOrEmpty(profileId)
                && !string.IsNullOrEmpty(eapConfigXml))
            {
                // TODO: ^perhaps reuse logic from PersistingStore.IsReinstallable
                Debug.WriteLine(nameof(eapConfigXml) + " was set", category: nameof(HandleProfileSelect));

                eapConfig = EapConfig.FromXmlData(profileId, eapConfigXml);
            }
            else
            {
                Debug.WriteLine(nameof(eapConfigXml) + " was not set", category: nameof(HandleProfileSelect));

                profile = IdpDownloader.GetProfileFromId(profileId);
                try
                {
                    eapConfig = await DownloadEapConfig(profile);
                }
                catch (EduroamAppUserError ex) // TODO: register this in some higher level
                {
                    MessageBox.Show(
                        ex.UserFacingMessage,
                        caption: "geteduroam - Exception");
                    eapConfig = null;
                }
            }

            // reenable buttons after LoadPageLoading() disables them
            btnBack.IsEnabled = true;
            btnNext.IsEnabled = true;

            if (eapConfig != null)
            {
                if (!CheckIfEapConfigIsSupported(eapConfig))
                    return false;

                if (HasInfo(eapConfig) && !skipOverview)
                {
                    LoadPageProfileOverview();
                    return true;
                }
                if (ConnectToEduroam.EnumerateCAInstallers(eapConfig)
                        .Any(installer => installer.IsInstalledByUs || !installer.IsInstalled))
                {
                    LoadPageCertificateOverview();
                    return true;
                }

                LoadPageLogin();
                return true;
            }
            else if (!string.IsNullOrEmpty(profile?.redirect))
            {
                // TODO: add option to go to selectmethod from redirect
                LoadPageRedirect(new Uri(profile.redirect));
                return true;
            }
            else if (profile?.oauth ?? false)
            {

                LoadPageOAuthWait(profile);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Used to determine if an eapconfig has enough info
        /// for the ProfileOverview page to show
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static bool HasInfo(EapConfig config)
            => !string.IsNullOrEmpty(config.InstitutionInfo.WebAddress)
            || !string.IsNullOrEmpty(config.InstitutionInfo.EmailAddress)
            || !string.IsNullOrEmpty(config.InstitutionInfo.Description)
            || !string.IsNullOrEmpty(config.InstitutionInfo.Phone)
            || !string.IsNullOrEmpty(config.InstitutionInfo.TermsOfUse);

        /// <summary>
        /// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
        /// </summary>
        private List<IdentityProviderProfile> GetProfiles(int providerId)
            => IdpDownloader.GetIdentityProviderProfiles(providerId);

        /// <summary>
        /// Gets EAP-config file, either directly or after browser authentication.
        /// Prepares for redirect if no EAP-config.
        /// </summary>
        /// <returns>EapConfig object.</returns>
        /// <exception cref="EduroamAppUserError">description</exception>
        public async Task<EapConfig> DownloadEapConfig(IdentityProviderProfile profile)
        {
            if (string.IsNullOrEmpty(profile?.Id))
                return null;

            // if OAuth
            if (profile.oauth || !string.IsNullOrEmpty(profile.redirect))
                return null;

            return await Task.Run(()
                => IdpDownloader.DownloadEapConfig(profile.Id)
            );
        }



        /// <summary>
        /// Tries to connect to eduroam
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> Connect()
        {
            bool connectSuccess;
            try
            {
                connectSuccess = await Task.Run(ConnectToEduroam.TryToConnect);
            }
            catch (EduroamAppUserError ex)
            {
                // NICE TO HAVE: log the error
                connectSuccess = false;
                MessageBox.Show("Could not connect. \nException: " + ex.UserFacingMessage);
            }
            return connectSuccess;
        }

        /// <summary>
        /// Checks if an EAP-config file exists in the same folder as the executable.
        /// If the installed and a EAP-config was bundled in a EXE using 7z, then this case will trigger
        /// </summary>
        /// <returns>EapConfig or null</returns>
        public static EapConfig GetBundledEapConfig()
        {
            string exeLocation = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] files = Directory.GetFiles(exeLocation, "*.eap-config");

            if (files.Length <= 0) return null;
            try
            {
                string eapPath = files.First(); // TODO: although correct, this seems smelly
                string eapString = File.ReadAllText(eapPath);
                var eapconfig = EapConfig.FromXmlData(profileId: null, eapString);

                return EduroamNetwork.EapConfigIsSupported(eapconfig)
                    ? eapconfig
                    : null;
            }
            catch (EduroamAppUserError)
            {
                return null;
            }
        }

        private static App App { get => (App)Application.Current; }

        public bool ShowNotification(string message, string title = "geteduroam", BalloonIcon icon = BalloonIcon.Info)
        {
            // TODO: doesn't show for peder, but does show for simon. RDP might be the culprit
            TaskbarIcon.ShowBalloonTip(title, message, icon);
            return true; // to be able to use it inside an expression
        }

        /// <summary>
        /// Called by the Menu.OAuthWait page when the OAuth process is done.
        /// Gives the EapConfig it got from the oauth session as param
        /// </summary>
        /// <param name="eapConfig"></param>
        public void OAuthComplete(EapConfig eapConfig)
        {
            if (!CheckIfEapConfigIsSupported(eapConfig))
                eapConfig = null;

            this.eapConfig = eapConfig;

            if (eapConfig != null)
            {
                if (HasInfo(eapConfig))
                {
                    LoadPageProfileOverview();
                }
                LoadPageCertificateOverview();
            }
            else
            {
                PreviousPage();
            }
            Activate();
        }

        /// <summary>
        /// Loads the logo form the curent eapconfig if it exists.
        /// Else display Eduroam logo.
        /// SVG not currently supported
        /// </summary>
        public void LoadProviderLogo()
        {
            ResetLogo();
            // gets institution logo encoded to base64
            byte[] logoBytes = eapConfig.InstitutionInfo.LogoData;
            string logoMimeType = eapConfig.InstitutionInfo.LogoMimeType;
            // adds logo to form if exists
            if (logoBytes.Length > 0)
            {
                // deactivate eduroam logo if institute has its own logo
                imgEduroamLogo.Visibility = Visibility.Hidden;
                // gets size of container
                int cWidth = (int)webLogo.Width;
                int cHeight = (int)webLogo.Height;

                if (logoMimeType == "image/svg+xml")
                {
                    imgEduroamLogo.Visibility = Visibility.Visible;
                    imgLogo.Visibility = Visibility.Hidden;
                    //webLogo.Visibility = Visibility.Visible;
                    //webLogo.NavigateToString(ImageFunctions.GenerateSvgLogoHtml(logoBytes, cWidth, cHeight));

                }
                else // other filetypes (jpg, png etc.)
                {
                    try
                    {
                        // converts from base64 to image                      
                        BitmapImage bitMapImage = ImageFunctions.LoadImage(logoBytes);
                        imgLogo.Source = bitMapImage;
                        imgLogo.Visibility = Visibility.Visible;
                    }
                    catch (System.FormatException)
                    {
                        // ignore
                    }
                }
            }
        }

        /// <summary>
        /// Empties both logo controls and makes them invisible.
        /// </summary>
        public void ResetLogo()
        {
            // reset pbxLogo
            //imgLogo.Source = null;
            imgLogo.Visibility = Visibility.Hidden;

            // reset webLogo
            //webLogo.Navigate("about:blank");
            //if (webLogo.Document != null)
            //{
            //    webLogo.NavigateToString(string.Empty);
            //}
            webLogo.Visibility = Visibility.Hidden;
            imgEduroamLogo.Visibility = Visibility.Visible;
        }

        public void LoadPageInstalledProfile()
        {
            currentFormId = FormId.InstalledProfile;
            btnBack.IsEnabled = false;
            btnBack.Visibility = Visibility.Hidden;
            btnNext.IsEnabled = true;
            btnNext.Visibility = Visibility.Visible;
            btnNext.Content = "No profile id";
            pageInstalledProfile = new InstalledProfile(this);
            Navigate(pageInstalledProfile);
        }

        public void LoadPageMainMenu(bool refresh = true)
        {
            PresetUsername = null;
            ExtractFlag = false;
            currentFormId = FormId.MainMenu;
            btnNext.Visibility = Visibility.Hidden;
            btnBack.Visibility = Visibility.Hidden;
            ResetLogo();
            if (refresh) pageMainMenu = new MainMenu(this);
            Navigate(pageMainMenu);
        }

        public void LoadPageSelectInstitution(bool refresh = true)
        {
            PresetUsername = null;
            currentFormId = FormId.SelectInstitution;
            btnNext.Visibility = Visibility.Visible;
            btnNext.Content = "Next";
            btnBack.IsEnabled = true;
            btnBack.Visibility = Visibility.Visible;
            ResetLogo();
            if (refresh) pageSelectInstitution = new SelectInstitution(this);

            Navigate(pageSelectInstitution);
        }

        public void LoadPageSelectProfile(bool refresh = true)
        {
            PresetUsername = null;
            currentFormId = FormId.SelectProfile;
            btnNext.Visibility = Visibility.Visible;
            btnNext.Content = "Next";
            ResetLogo();
            if (refresh) pageSelectProfile = new SelectProfile(this, pageSelectInstitution.IdProviderId);
            Navigate(pageSelectProfile);
        }

        public void LoadPageProfileOverview(bool refresh = true)
        {
            currentFormId = FormId.ProfileOverview;
            btnNext.Visibility = Visibility.Visible;
            btnNext.IsEnabled = true;
            btnNext.Content = "Next";
            btnBack.Visibility = Visibility.Visible;
            if (refresh) pageProfileOverview = new ProfileOverview(this, eapConfig);
            Navigate(pageProfileOverview);
        }

        public void LoadPageCertificateOverview(bool refresh = true)
        {
            // if all certificates are installed then skip to login
            currentFormId = FormId.CertificateOverview;
            btnBack.Visibility = Visibility.Visible;
            btnBack.IsEnabled = true;
            btnNext.Content = "Next";
            if (refresh) pageCertificateOverview = new CertificateOverview(this, eapConfig);
            Navigate(pageCertificateOverview);
        }

        public void LoadPageLogin(bool refresh = true)
        {
            currentFormId = FormId.Login;
            btnBack.IsEnabled = true;
            btnBack.Visibility = Visibility.Visible;
            if (refresh) pageLogin = new Login(this, eapConfig);
            Navigate(pageLogin);
        }

        public void LoadPageRedirect(Uri redirect, bool refresh = true)
        {
            currentFormId = FormId.Redirect;
            btnBack.IsEnabled = true;
            btnNext.IsEnabled = false;
            if (refresh) pageRedirect = new Redirect(this, redirect);
            Navigate(pageRedirect);
        }

        public void LoadPageLoading(bool refresh = true)
        {
            currentFormId = FormId.Loading;
            btnBack.IsEnabled = false;
            btnNext.IsEnabled = false;
            if (refresh) pageLoading = new Loading(this);
            Navigate(pageLoading);
        }


        public void LoadPageTermsOfUse(bool refresh = true)
        {
            currentFormId = FormId.TermsOfUse;
            btnBack.IsEnabled = true;
            btnNext.Content = "OK";
            btnNext.Visibility = Visibility.Visible;
            if (refresh) pageTermsOfUse = new TermsOfUse(this, eapConfig.InstitutionInfo.TermsOfUse);
            Navigate(pageTermsOfUse);
        }

        public void LoadPageOAuthWait(IdentityProviderProfile profile)
        {
            currentFormId = FormId.OAuthWait;
            btnBack.IsEnabled = true;
            btnBack.Visibility = Visibility.Visible;
            btnNext.IsEnabled = false;
            pageOAuthWait = new OAuthWait(this, profile);
            Navigate(pageOAuthWait);
        }

        

        private bool IsShuttingDown = false;
        public void Shutdown(int exitCode = 0)
        {
            IsShuttingDown = true;
            Application.Current.Shutdown(exitCode);
        }


        private void btnNext_Click(object sender, RoutedEventArgs e)
            => NextPage();

        private void btnBack_Click(object sender, RoutedEventArgs e)
            => PreviousPage();

        // Logic to minimize to tray:

        private void OnWindowClose(object sender, CancelEventArgs e)
        {
            Debug.WriteLine("Event: OnClose");
            Debug.WriteLine("Sender: " + sender.ToString());
            Debug.WriteLine("{0}: {1}", nameof(IsShuttingDown), IsShuttingDown);
            Debug.WriteLine("{0}: {1}", nameof(App.Installer.IsInstalled), App.Installer.IsInstalled);
            Debug.WriteLine("{0}: {1}", nameof(App.Installer.IsRunningInInstallLocation), App.Installer.IsRunningInInstallLocation);

            if (!App.Installer.IsInstalled)
                return; // do not cancel the Closing event

            if (App.Installer.IsInstalled && !App.Installer.IsRunningInInstallLocation)
            {
                #if !DEBUG
                // this happens after the first time setup
                SelfInstaller.DelayedStart(App.Installer.StartMinimizedCommand);
                #endif
                return; // do not cancel the Closing event
            }

            if (IsShuttingDown)
                return; // closed in tray icon, unable to cancel. avoid creating the balloon

            // Cancels the Window.Close(), but unable to cancel Application.Shutdown()
            e.Cancel = true;

            ShowNotification("geteduroam is still running in the background");

            Hide(); // window

            if (PersistingStore.IdentityProvider != null)
                LoadPageInstalledProfile();
            else
                LoadPageMainMenu();

            historyFormId.Clear();
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
            => tb_TrayLeftMouseDown(sender, e);

        private void tb_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Event: TrayLeftMouseDown");
            if (!IsVisible)
            {
                Show(); // window
                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;
                Activate(); // focus window
            }
            else
            {
                Hide(); // window
            }
        }

        private void MenuItem_Click_Show(object sender, RoutedEventArgs e)
		{
            Debug.WriteLine("Event: MenuItem_Click_Show");
            Show(); // window
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            Activate(); // focus window
        }

        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
            => Shutdown();


        /// <summary>
        /// Disables WPF history nagivation.
        /// </summary>
		private void Main_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
		{
            if (e.NavigationMode != NavigationMode.New)
                e.Cancel = true;
		}
	}
}
