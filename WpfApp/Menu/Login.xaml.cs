using EduroamConfigure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace WpfApp.Menu
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {


        private readonly EapConfig.AuthenticationMethod authMethod;


        private bool usernameValid = false;
        private string realm;
        private bool hint;
        private Control focused;
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
            gridCred.Visibility = Visibility.Collapsed;
            gridCert.Visibility = Visibility.Collapsed;
            eapConfig.AuthenticationMethods.First();

            mainWindow.btnNext.Content = "Connect";

            if (eapConfig.NeedsLoginCredentials())
            {
                // TODO: show input fields
                grpRules.Visibility = Visibility.Hidden;
                gridCred.Visibility = Visibility.Visible;
                tbRules.Visibility = Visibility.Visible;
                (realm, hint) = eapConfig.GetClientInnerIdentityRestrictions();
                tbRealm.Text = '@' + realm;
                tbRealm.Visibility = !string.IsNullOrEmpty(realm) && hint ? Visibility.Visible : Visibility.Hidden;
                tbUsername.Focus();              
                ValidateFields();
            }
            else if (eapConfig.NeedsClientCertificate())
            {

            }
            else if (eapConfig.NeedsClientCertificatePassphrase())
            {
                // TODO: show input field
                // This field should write to this:
                gridCred.Visibility = Visibility.Visible;
                var success = eapConfig.AddClientCertificatePassphrase("asd");
            }
            else
            {

            }
        }

        public bool ValidateFields()
        {
            string username = tbUsername.Text;
            if (string.IsNullOrEmpty(username))
            {
                usernameValid = false;
                tbRules.Text = "";
                mainWindow.btnNext.IsEnabled = false;
                return false;
            }

            // if username does not contain '@' and realm is given then show realm added to end
            if ((!username.Contains('@') && !string.IsNullOrEmpty(realm)) || hint)
            {
                username += "@" + realm;
            }


            var brokenRules = IdentityProviderParser.GetRulesBroken(username, realm, hint).ToList();
            usernameValid = !brokenRules.Any();
            tbRules.Text = "";
            if (!usernameValid)
            {
                tbRules.Text = string.Join("\n", brokenRules); ;
            }

            bool fieldsValid = (!string.IsNullOrEmpty(pbCredPassword.Password) && usernameValid) || IsConnected;
            mainWindow.btnNext.IsEnabled = fieldsValid;
            return fieldsValid;
        }

        public void ConnectClick()
        {
            if (ValidateFields())
            {
                if ((!tbUsername.Text.Contains('@') && !string.IsNullOrEmpty(realm)) || hint)
                {
                    tbRealm.Visibility = Visibility.Visible;
                }
                ConnectWithLogin();
                return;
            }
            grpRules.Visibility = string.IsNullOrEmpty(tbRules.Text) ? Visibility.Hidden : Visibility.Visible;
        }


        public async void ConnectWithLogin()
        {
            string username = tbUsername.Text;
            if (tbRealm.Visibility == Visibility.Visible)
            {
                username += tbRealm.Text;
            }
            string password = pbCredPassword.Password;

            mainWindow.btnNext.IsEnabled = false;
            tbStatus.Text = "Connecting...";
            tbStatus.Visibility = Visibility.Visible;
            pbCredPassword.IsEnabled = false;
            tbUsername.IsEnabled = false;
            bool installed = await Task.Run(() => InstallEapConfig(eapConfig, username, password));
            if (installed)
            {
                Connect();
            }
            else
            {
                tbStatus.Text = "Connection to eduroam failed.";
                pbCredPassword.IsEnabled = true;
                tbUsername.IsEnabled = true;
                mainWindow.btnNext.IsEnabled = true;
                focused.Focus();
            }



        }

        private async void Connect()
        {

            bool eduConnected = await Task.Run(AsyncConnect);

            if (eduConnected)
            {
                tbStatus.Text = "You are now connected to eduroam.\n\nPress Close to exit the wizard.";
                mainWindow.btnNext.Content = "Close";
            }
            else
            {
                tbStatus.Text = "Connection to eduroam failed.";
            }
            IsConnected = eduConnected;

            pbCredPassword.IsEnabled = true;
            tbUsername.IsEnabled = true;
            mainWindow.btnNext.IsEnabled = true;
            focused.Focus();
        }

        public async Task<bool> AsyncConnect()
        {
            bool connectSuccess;
            try
            {
                connectSuccess = await Task.Run(ConnectToEduroam.TryToConnect);
            }
            catch (Exception ex)
            {
                connectSuccess = false;
                MessageBox.Show("Could not connect. \nException: " + ex.Message);
            }
            return connectSuccess;
        }


        /// <summary>
        /// Installs certificates from EapConfig and creates wireless profile.
        /// </summary>
        /// <returns>true on success</returns>
        private bool InstallEapConfig(EapConfig eapConfig, string username = null, string password = null) // TODO: make static
        {
            if (!EduroamNetwork.EapConfigIsSupported(eapConfig))
            {
                MessageBox.Show(
                    "The profile you have selected is not supported by this application.",
                    "No supported authentification method ws found.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }


            // test
            ConnectToEduroam.RemoveAllProfiles();
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
                    if (!authMethodInstaller.InstallProfile(username, password))
                        continue; // failed, try the next method

                    success = true;
                    break;
                }

                // TODO: move this out of function
                if (!EduroamNetwork.IsEduroamAvailable(eapConfig))
                {
                    //err = "eduroam not available";
                }

                // TODO: move out of function, use return value. This function should be static
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

        private void tbUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbStatus.Visibility = Visibility.Hidden;
            grpRules.Visibility = Visibility.Hidden;
            if (!hint) tbRealm.Visibility = Visibility.Hidden;
            ValidateFields();
        }

        private void tbUsername_LostFocus(object sender, RoutedEventArgs e)
        {
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
            if (IgnorePasswordChange) return;
            tbCredPassword.Text = string.IsNullOrEmpty(pbCredPassword.Password) ? "" : "something";
            tbStatus.Visibility = Visibility.Hidden;
            ValidateFields();
        }

        private void pbCredPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            focused = pbCredPassword;
        }

    }
}
