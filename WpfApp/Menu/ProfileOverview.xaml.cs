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
using System.Diagnostics;

namespace WpfApp.Menu
{
    /// <summary>
    /// Interaction logic for ProfileOverview.xaml
    /// </summary>
    public partial class ProfileOverview : Page
    {
        private readonly MainWindow mainWindow;
        private readonly EapConfig eapConfig;
        
        public ProfileOverview(MainWindow mainWindow, EapConfig eapConfig)
        {
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
            this.eapConfig = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));
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
            string phone = eapConfig.InstitutionInfo.Phone;

            // displays website, email, phone number
            lblPhone.Content = phone;

            // checks if website url is valid
            bool isValidUrl = Uri.TryCreate(webAddress, UriKind.Absolute, out Uri uriResult)
                                  && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // show url as link
            if (isValidUrl)
            {

                tbWebLink.Text = webAddress;
                hlinkWeb.NavigateUri = new Uri(webAddress);
                hlinkWeb.TextDecorations = null;

            }
            // show url but not as link
            else
            {
                tbWebText.Text = webAddress;
            }

            // checks if email address is valid
            bool isValidEmail = !(emailAddress.Contains(" ") || !emailAddress.Contains("@"));
            // show url as link
            if (isValidEmail)
            {
                tbEmailLink.Text = emailAddress;
                hlinkEmail.NavigateUri = new Uri("mailto:"+emailAddress);
                hlinkEmail.TextDecorations = null;

             /*   if (emailAddress.Contains("******"))
                {
                    lblEmail.Texst = "-";
                }*/
            }
            // show url but not as link
            else
            {
                tbEmailText.Text = emailAddress;
            }
            
            // checks if phone number has numbers, disables label if not
          /*  if (!lblPhone.Text.Any(char.IsDigit)) lblPhone.Enabled = false;

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

        private void LinkClick(object sender, RequestNavigateEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }
        private void Hyperlink_TOU(object sender, RequestNavigateEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }

    }
}
