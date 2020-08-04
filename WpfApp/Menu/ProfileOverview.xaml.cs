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
using System.Security.Cryptography;
using EduroamConfigure;

namespace WpfApp.Menu
{
    /// <summary>
    /// Interaction logic for ProfileOverview.xaml
    /// </summary>
    public partial class ProfileOverview : Page
    {
        private readonly MainWindow mainWindow;
        private EapConfig eapConfig;
        
        public ProfileOverview(MainWindow mainWindow, EapConfig eapConfig)
        {
            this.mainWindow = mainWindow;
            this.eapConfig = eapConfig;
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            
            tbDesc.Text = eapConfig.InstitutionInfo.Description;
            tbDesc.Visibility = string.IsNullOrEmpty(tbDesc.Text) ? Visibility.Collapsed : Visibility.Visible;
            tbName.Text = eapConfig.InstitutionInfo.DisplayName;
            LoadContactInfo();
            LoadTOU();
            LoadAlternate();

        }

        private void LoadContactInfo()
        {
            string webAddress = eapConfig.InstitutionInfo.WebAddress;
            string emailAddress = eapConfig.InstitutionInfo.EmailAddress;
           
            // displays website, email, phone number
            lblWeb.Content = !string.IsNullOrEmpty(webAddress) ? webAddress : "-";
            lblEmail.Content = emailAddress;
            lblPhone.Content = eapConfig.InstitutionInfo.Phone;
/*
            // checks if website url is valid
            bool isValidUrl = Uri.TryCreate(webAddress, UriKind.Absolute, out Uri uriResult)
                                  && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isValidUrl)
            {
                // sets linkdata
                var redirectLink = new LinkLabel.Link { LinkData = webAddress };
                lblWeb.Links.Add(redirectLink);
            }
            // disables link, but still displays it
            else
            {
                lblWeb.Enabled = false;
            }

            // checks if email address is valid
            if (emailAddress.Contains(" ") || !emailAddress.Contains("@"))
            {
                // disables link, but still displays it
                lblEmail.Enabled = false;

                if (emailAddress.Contains("******"))
                {
                    lblEmail.Text = "-";
                }
            }

            // checks if phone number has numbers, disables label if not
            if (!lblPhone.Text.Any(char.IsDigit)) lblPhone.Enabled = false;

            // replaces empty fields with a dash
            foreach (Control cntrl in tblContactInfo.Controls)
            {
                if (string.IsNullOrEmpty(cntrl.Text))
                {
                    cntrl.Text = "-";
                }
            }
*/
        }

        private void LoadTOU()
        {
            string tou = eapConfig.InstitutionInfo.TermsOfUse;

            if (string.IsNullOrEmpty(tou))
            {
                tbTou.Visibility = Visibility.Collapsed;
            }
            else
            {
               // termsOfUse = tou;
               // lnkToU.Visible = true;


            }
           
        }

        private void LoadAlternate()
        {
            gridAlt.Visibility = Visibility.Collapsed;
            if (true)
            {
                tbAlt.Text = $"Not affiliated with {eapConfig.InstitutionInfo.DisplayName}?";
            }
            else
            {
                gridAlt.Visibility = Visibility.Collapsed;
            }
        }

        // TODO: fix hyperlink and show ToU to user
        private void Hyperlink_TOU(object sender, RequestNavigateEventArgs e)
        {
            tbTou.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Installs certificates from EapConfig and creates wireless profile.
        /// </summary>
        /// <returns>
        /// A tuple containing:
        ///     - the auth method which succeded installing, or null
        ///     - a string describing why the installation failed
        /// </returns>
        public ValueTuple<EapConfig.AuthenticationMethod, string> InstallEapConfig()
        {
            try
            {
                EapConfig.AuthenticationMethod authMethod = null;
                string err = "nothing installed"; // TODO: enum?

                if (!ConnectToEduroam.EapConfigIsSupported(eapConfig))
                {
                    MessageBox.Show(
                        "The profile you have selected is not supported by this application.",
                        "No supported authentification method found.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return (authMethod, "not supported");
                }

                // Install EAP config as a profile
                foreach (var authMethodInstaller in ConnectToEduroam.InstallEapConfig(eapConfig))
                {

                    // TODO: create page or something that handles this

                    // check if we need to find a client certificate first
                  /*  if (authMethodInstaller.NeedsClientCertificate())
                    {
                        MessageBoxResult dialogResult = MessageBox.Show(
                            "The selected profile prefers a separately provided client certificate. Do you want to browse your local files for one?",
                            "Client certificate needed", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (dialogResult != MessageBoxResult.Yes) continue; // try some other auth method

                        var results = FileDialog.AskUserForClientCertificateBundle();
                        if (results == null) continue; // user aborted

                        (string certPath, string certPass) = results.Value;
                        authMethodInstaller.AddClientCertificate(certPath, certPass); // todo, check success
                    }
                  */
                    // warn user if we need to install any CAs
                    if (authMethodInstaller.NeedsToInstallCAs())
                        MessageBox.Show(
                            "You will now be prompted to install a Certificate Authority. \n" +
                            "In order to connect to eduroam, you need to accept this by pressing \"Yes\" in the following dialog.",
                            "Accept Certificate Authority", MessageBoxButton.OK);

                    // install CAs
                    while (!authMethodInstaller.InstallCertificates())
                    {
                        // Ask user if he wants to retry
                        MessageBoxResult retryCa = MessageBox.Show(
                            "CA not installed. \n" +
                            "In order to connect to eduroam, you must press \"Yes\" when prompted to install the Certificate Authority.",
                            "Accept Certificate Authority", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                        if (retryCa == MessageBoxResult.Cancel)
                            break;
                    }
                    if (authMethodInstaller.NeedsToInstallCAs()) break; // if user refused to install the CAs, abort

                    // Everything is now in order, install the profile!
                    if (authMethodInstaller.InstallProfile())
                    {
                        // installation was a success!
                        authMethod = authMethodInstaller.AuthMethod;
                        err = null;
                        break;
                    }
                }

                if (!EduroamNetwork.IsEduroamAvailable(eapConfig))
                {
                    err = "eduroam not available";
                }

                mainWindow.ProfileCondition = MainWindow.ProfileStatus.Incomplete;

                return (authMethod, err);
            }
            catch (ArgumentException argEx) // TODO, handle in ConnectToEduroam or EduroamNetwork
            {
                if (argEx.Message == "interfaceId")
                {
                    // was this due to Guid.Empty, in that case this should be solved
                    MessageBox.Show(
                        "Could not establish a connection through your computer's wireless network interface.\n" +
                        "Please go to Control Panel -> Network and Internet -> Network Connections to make sure that it is enabled.\n" +
                        "\n" +
                        "Exception: " + argEx.Message,
                        "eduroam", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                throw;
            }
            catch (CryptographicException cryptEx) // TODO, handle in ConnectToEduroam, thrown by certificate store .add()
            {
                MessageBox.Show(
                    "One or more certificates are corrupt. Please select an another file, or try again later.\n" +
                    "\n" +
                    "Exception: " + cryptEx.Message,
                    "eduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (EduroamAppUserError ex)
            {
                MessageBox.Show(
                    ex.UserFacingMessage,
                    "eduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex) // TODO, handle in ConnectToEduroam or EduroamNetwork
            {
                MessageBox.Show(
                    "Something went wrong.\n" +
                    "Please try connecting with another institution, or try again later.\n" +
                    "\n" +
                    "Exception: " + ex.Message,
                    "eduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            return (null, "exception occured");
        }
    }
}
