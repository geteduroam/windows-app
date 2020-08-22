using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using EduroamConfigure;
using WpfApp.Classes;

namespace WpfApp.Menu
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {

        private enum ConType
        {
            Credentials,
            CertPass,
            CertAndCertPass,
            Nothing,
        }
        private ConType conType;
        private string realm;
        private bool hint;
        private Control focused;
        private string filepath;
        private DateTime certValid;
        public DispatcherTimer dispatcherTimer { get; set; }
        public bool IsConnected { get; set; }
        public bool IgnorePasswordChange { get; set; }
        private readonly MainWindow mainWindow;
        private readonly EapConfig eapConfig;



        public Login(MainWindow mainWindow, EapConfig eapConfig)
        {
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
            this.eapConfig = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            // Collaps everything before deciding what to show
            gridCred.Visibility = Visibility.Collapsed;
            gridCertPassword.Visibility = Visibility.Collapsed;
            gridCertBrowser.Visibility = Visibility.Collapsed;
            stpTime.Visibility = Visibility.Collapsed;

            mainWindow.btnNext.IsEnabled = false;
            mainWindow.btnNext.Content = "Connect";

            // create dispatcherTimer used for counting up cerst if not active yet
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            // Case where eapconfig needs username+passwprd
            if (eapConfig.NeedsLoginCredentials())
            {
                conType = ConType.Credentials;
                grpRules.Visibility = Visibility.Hidden;
                gridCred.Visibility = Visibility.Visible;
                tbRules.Visibility = Visibility.Visible;
                (realm, hint) = eapConfig.GetClientInnerIdentityRestrictions();
                tbRealm.Text = '@' + realm;
                tbRealm.Visibility = !string.IsNullOrEmpty(realm) && hint ? Visibility.Visible : Visibility.Hidden;
                if (!string.IsNullOrEmpty(mainWindow.PresetUsername))
                {
                    // take username@realm in its entirety
                    tbUsername.Text = mainWindow.PresetUsername;
                    // if no subrealms allowed so login screen will always have @realm added to whatever is written in username textbox
                    if (hint)
                    {
                        if (mainWindow.PresetUsername.EndsWith(realm, StringComparison.InvariantCulture) && mainWindow.PresetUsername.Contains('@'))
                        {
                            //take username before realm and put in textbox
                            tbUsername.Text = mainWindow.PresetUsername.Split('@').FirstOrDefault() ?? "";
                        }
                    }
                    pbCredPassword.Focus();
                }
                else
                {
                    tbUsername.Focus();
                }
                
                EnableConnectBtnBasedOnCredentials();
            }
            // case where eapconfig needs a certificate and password
            else if (eapConfig.NeedsClientCertificate())
            {
                gridCertBrowser.Visibility = Visibility.Visible;
                conType = ConType.CertAndCertPass;
                
            }
            // case where eapconfig needs only cert password
            else if (eapConfig.NeedsClientCertificatePassphrase())
            {
                conType = ConType.CertPass;
                gridCertPassword.Visibility = Visibility.Visible;
                mainWindow.btnNext.IsEnabled = true;
            }
            // case where no extra info is needed to connect
            else
            {
                // just connnect
                conType = ConType.Nothing;
                ConnectClick();
            }
        }

        /// <summary>
        /// Determines if the entered username/password is valid
        /// for the case where username/password is needed.
        /// Also saves any brokenrules to the tbRules textblock
        /// </summary>
        /// <returns>true if username/password legal</returns>
        private bool IsCredentialsValid()
        {

            string username = tbUsername.Text;
            if (string.IsNullOrEmpty(username))
            {
                tbRules.Text = "";
                mainWindow.btnNext.IsEnabled = false;
                return false;
            }

            // if username does not contain '@' and realm is given then show realm added to end
            if ((!username.Contains('@') && !string.IsNullOrEmpty(realm)) || hint)
            {
                username += "@" + realm;
                tbRealm.Visibility = Visibility.Visible;
            }

            var brokenRules = IdentityProviderParser.GetRulesBrokenOnUsername(username, realm, hint).ToList();
            bool usernameValid = !brokenRules.Any();
            tbRules.Text = "";
            if (!usernameValid)
            {
                tbRules.Text = string.Join("\n", brokenRules);
            }

            bool fieldsValid = (!string.IsNullOrEmpty(pbCredPassword.Password) && usernameValid) || IsConnected;
            return fieldsValid;
        }
        /// <summary>
        /// Decides if user is allowed to attempt to connect.
        /// Based on if filepath and password is set
        /// </summary>
        /// <returns>True if filepath and password is set</returns>
        public void ValidateCertBrowserFields()
        {
            mainWindow.btnNext.IsEnabled = !string.IsNullOrEmpty(filepath);
        }

        /// <summary>
        /// Function used to attempt to connect to eduroam. This is the 'entry' point and decides what logic
        /// to use next based on the current case
        /// </summary>
        public async void ConnectClick()
        {
            mainWindow.btnBack.IsEnabled = false;
            mainWindow.btnNext.IsEnabled = false;
            tbStatus.Text = "Connecting...";
            IgnorePasswordChange = true;
            tbStatus.Visibility = Visibility.Visible;

            switch (conType)
            {
                case ConType.Credentials:
                    await ConnectWithLogin();
                    break;
                case ConType.Nothing:
                    await ConnectWithNothing();
                    break;
                case ConType.CertAndCertPass:
                    await ConnectWithCertAndCertPass();
                    break;
                case ConType.CertPass:
                    await ConnectWithCertPass();
                    break;
            }
            if (!dispatcherTimer.IsEnabled)
            {
                mainWindow.btnNext.IsEnabled = true;
            }
            mainWindow.btnBack.IsEnabled = true;
            IgnorePasswordChange = false;

        }

        /// <summary>
        /// Used if both certificate and certificate password is needed
        /// </summary>
        /// <returns>truereturns>
        public async Task<bool> ConnectWithCertAndCertPass()
        {
            var success = eapConfig.AddClientCertificate(filepath, pbCertBrowserPassword.Password);

            if (success)
            {
                _ = await ConnectAndUpdateUI();
            }
            else
            {
                tbStatus.Text = "Incorrect password";
            }
            return true;

        }

        /// <summary>
        /// Used if only a certificate password is needed
        /// </summary>
        /// <returns>true</returns>
        public async Task<bool> ConnectWithCertPass()
        {
            var success = eapConfig.AddClientCertificatePassphrase(pbCertPassword.Password);

            if (success)
            {
                pbCertPassword.IsEnabled = false;

                _ = await ConnectAndUpdateUI();
                pbCertPassword.IsEnabled = true;
            }
            else
            {
                tbStatus.Text = "Incorrect password";
            }
            return true;

        }

        /// <summary>
        /// Used if username and password is needed
        /// </summary>
        /// <returns>true</returns>
        public async Task<bool> ConnectWithLogin()
        {
            if (IsCredentialsValid())
            {
                string username = tbUsername.Text;


                if (tbRealm.Visibility == Visibility.Visible)
                {
                    username += tbRealm.Text;
                }
                string password = pbCredPassword.Password;

                pbCredPassword.IsEnabled = false;
                tbUsername.IsEnabled = false;

                _ = await ConnectAndUpdateUI(username, password);

                pbCredPassword.IsEnabled = true;
                tbUsername.IsEnabled = true;

                if (focused != null) focused.Focus();
            }
            else
            {
                grpRules.Visibility = string.IsNullOrEmpty(tbRules.Text) ? Visibility.Collapsed : Visibility.Visible;
                tbStatus.Text = "";
            }
            return true;
        }

        /// <summary>
        /// Used if no extra credentials are needed to connect
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectWithNothing()
        {
            _ = await ConnectAndUpdateUI();
            return true;
        }

        /// <summary>
        /// Common function used by all the various connection cases to install the eap config and actually connect
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAndUpdateUI(string username = null, string password = null)
        {
            pbCertBrowserPassword.IsEnabled = false;
            bool installed = await Task.Run(() => InstallEapConfig(eapConfig, username, password));
            if (installed)
            {
                // Any profile installed by us must also be removed by us when it is not needed anymore
                // so install the geteduroam app when we have installed a profile
                _ = Task.Run(App.Installer.EnsureIsInstalled); // TODO: must be ensured to complete before the user exits

                bool connected = await TryToConnect();
                if (connected)
                {
                    tbStatus.Text = "You are now connected to eduroam.\n\nPress Close to exit the wizard.";
                    mainWindow.btnNext.Content = "Close";
                }
                else
                {
                    if (EduroamNetwork.IsEduroamAvailable(eapConfig))
                    {
                        tbStatus.Text = "Everything is configured!\nUnable to connect to eduroam.";
                    }
                    else
                    {
                        // Hs2 is not enumerable
                        tbStatus.Text = "Everything is configured!\nUnable to connect to eduroam, you're probably out of coverage.";
                    }
                    mainWindow.btnNext.Content = "Connect";
                }
            }
            else
            {
                tbStatus.Text = EduroamNetwork.IsWlanServiceApiAvailable()
                    ? "Could not install any profile due to there not seemingly being any wireless interfaces on this host." // TODO: update this message when wireless x802 is a thing
                    : "Could not install EAP-configuration";

                mainWindow.btnNext.Content = "Connect";
            }
            pbCertBrowserPassword.IsEnabled = true;


            return true; // to make it await-able
        }

        /// <summary>
        /// Tries to connect to eduroam with profiles registered previously with InstallEapConfig
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TryToConnect()
        {
            bool connectSuccess;
            try
            {
                connectSuccess = await Task.Run(ConnectToEduroam.TryToConnect);
            }
            catch (EduroamAppUserError ex)
            {
                connectSuccess = false;
                MessageBox.Show("Could not connect. \nException: " + ex.UserFacingMessage);
            }
            IsConnected = connectSuccess;
            return connectSuccess;
        }


        /// <summary>
        /// Installs certificates from EapConfig and creates wireless profile.
        /// </summary>
        /// <returns>true on success</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Catch-all to not let the application crash")]
        private bool InstallEapConfig(EapConfig eapConfig, string username = null, string password = null)
        {
            if (!MainWindow.CheckIfEapConfigIsSupported(eapConfig)) // should have been caught earlier, but check here too for sanity
                return false;

            ConnectToEduroam.RemoveAllWLANProfiles();
            mainWindow.ProfileCondition = MainWindow.ProfileStatus.NoneConfigured;

            bool success = false;

            try
            {
                // Install EAP config as a profile
                foreach (var authMethodInstaller in ConnectToEduroam.InstallEapConfig(eapConfig))
                {
                    // install intermediate CAs and client certificates
                    // if user refuses to install a root CA (should never be prompted to at this stage), abort
                    if (!authMethodInstaller.InstallCertificates())
                        break;

                    // Everything is now in order, install the profile!
                    if (!authMethodInstaller.InstallWLANProfile(username, password))
                        continue; // failed, try the next method

                    // check if we need to wait for the certificate to become valid
                    certValid = authMethodInstaller.GetTimeWhenValid().From;
                    if (DateTime.Now <= certValid)
                    {
                        // dispatch the event which creates the clock the end user sees
                        dispatcherTimer_Tick(dispatcherTimer, new EventArgs());
                        dispatcherTimer.Start();
                        return false;
                    }

                    success = true;
                    break;
                }

                mainWindow.ProfileCondition = MainWindow.ProfileStatus.Configured;

                return success;
            }
            catch (EduroamAppUserError ex)
            {
                // TODO: expand the response with "try something else"
                MessageBox.Show(
                    ex.UserFacingMessage,
                    "geteduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Something went wrong.\n" +
                    "Please try connecting with another profile or institution, or try again later.\n" +
                    "\n" +
                    "Exception: " + ex.Message,
                    "geteduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            return false;
        }

        /// <summary>
        /// Called once every second as long as the current certificate is not yet active.
        /// The timer counts the time to when the cert is valid, and disables the connect button
        /// as long as the cert is not active
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // update time on screen
            this.Dispatcher.Invoke(() => {
                tbLocalTime.Text = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                tbValidTime.Text = certValid.ToString(CultureInfo.InvariantCulture);
            });
            // if certificate has become valid then try to connect
            if (DateTime.Now > certValid)
            {
                dispatcherTimer.Stop();
                this.Dispatcher.Invoke(() => {
                    stpTime.Visibility = Visibility.Collapsed;
                    ConnectClick();
                });
            }
            // if still not vaid yet
            else
            {
                this.Dispatcher.Invoke(() => {
                    mainWindow.btnNext.IsEnabled = false;
                    stpTime.Visibility = Visibility.Visible;
                    tbStatus.Visibility = Visibility.Collapsed;
                });
            }
        }

        private void tbUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbStatus.Visibility = Visibility.Collapsed;
            grpRules.Visibility = Visibility.Collapsed;
            if (!hint) tbRealm.Visibility = Visibility.Hidden;
            EnableConnectBtnBasedOnCredentials();
        }

        /// <summary>
        /// Enables Connect button if username and password set, this should only be used
        /// for the case where username and password is needed
        /// </summary>
        private void EnableConnectBtnBasedOnCredentials()
        {
            mainWindow.btnNext.IsEnabled =
                !string.IsNullOrEmpty(tbUsername.Text) &&
                !string.IsNullOrEmpty(pbCredPassword.Password);
        }

        private void tbUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            IsCredentialsValid();
            grpRules.Visibility = string.IsNullOrEmpty(tbRules.Text) ? Visibility.Hidden : Visibility.Visible;
            if (!tbUsername.Text.Contains('@') && !string.IsNullOrEmpty(realm) && !string.IsNullOrEmpty(tbUsername.Text))
            {
                tbRealm.Visibility = Visibility.Visible;
            }
        }

        private void tbUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            focused = tbUsername;
        }

        private void pbCredPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // show placeholder if no password, hide placeholder if password set.
            // in XAML a textblock is bound to tbCredPassword so when the textbox is blank a placeholder is shown
            tbCredPassword.Text = string.IsNullOrEmpty(pbCredPassword.Password) ? "" : "something";
            // ignore unwanted PasswordChanged event
            if (IgnorePasswordChange) return;
            tbStatus.Visibility = Visibility.Hidden;
            EnableConnectBtnBasedOnCredentials();
        }

        private void pbCredPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            focused = pbCredPassword;
        }

        private void pbCertPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // show placeholder if no password, hide placeholder if password set.
            // in XAML a textblock is bound to tbCertPassword so when the textbox is blank a placeholder is shown
            tbCertPassword.Text = string.IsNullOrEmpty(pbCertPassword.Password) ? "" : "something";
            // ignore unwanted PasswordChanged event
            if (IgnorePasswordChange) return;
            tbStatus.Visibility = Visibility.Hidden;
        }

        private void pbCertPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            focused = pbCertPassword;
        }

        private void pbCertBrowserPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // show placeholder if no password, hide placeholder if password set.
            // in XAML a textblock is bound to tbCertBrowserPassword so when the textbox is blank a placeholder is shown
            tbCertBrowserPassword.Text = string.IsNullOrEmpty(pbCertBrowserPassword.Password) ? "" : "something";
            // ignore unwanted PasswordChanged event
            if (IgnorePasswordChange) return;
            tbStatus.Visibility = Visibility.Hidden;
            ValidateCertBrowserFields();
        }

        private void pbCertBrowserPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            focused = pbCertBrowserPassword;

        }

        // opens file browser for choosing a certificate
        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            // browse for certificate and add to eapconfig
            var filepath = FileDialog.AskUserForClientCertificateBundle();
            if (string.IsNullOrEmpty(filepath)) return;
            this.filepath = filepath;
            tbCertBrowser.Text = filepath;
            // scroll to end of textbox
            tbCertBrowser.CaretIndex = tbCertBrowser.Text.Length;
            var rect = tbCertBrowser.GetRectFromCharacterIndex(tbCertBrowser.CaretIndex);
            tbCertBrowser.ScrollToHorizontalOffset(rect.Right);
            ValidateCertBrowserFields();
        }

        private void btnCancelWait_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            stpTime.Visibility = Visibility.Collapsed;
            tbStatus.Visibility = Visibility.Visible;
            tbStatus.Text = "Connecting process was cancelled";
            mainWindow.btnNext.IsEnabled = true;
        }
    }
}
