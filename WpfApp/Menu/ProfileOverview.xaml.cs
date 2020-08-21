using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Diagnostics;
using EduroamConfigure;

namespace WpfApp.Menu
{
    /// <summary>
    /// Interaction logic for ProfileOverview.xaml
    /// </summary>
    public partial class ProfileOverview : Page
    {
        private readonly MainWindow mainWindow;
        private readonly EapConfig eapConfig;
        private readonly bool extractFlag;
        public bool ShowTou { get; set; }
        
        public ProfileOverview(MainWindow mainWindow, EapConfig eapConfig)
        {
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
            this.eapConfig = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));
            this.extractFlag = mainWindow.ExtractFlag;
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
            mainWindow.LoadProviderLogo();

        }

        private void LoadContactInfo()
        {
            if (!HasContactInfo(eapConfig.InstitutionInfo))
            {
                grpInfo.Visibility = Visibility.Collapsed;
                return;
            }

            LoadWeb();
            LoadEmail();
            LoadPhone();
        }

        private void LoadWeb()
        {
            string webAddress = eapConfig.InstitutionInfo.WebAddress;
            if (string.IsNullOrEmpty(webAddress))
            {
                tbWebText.Visibility = Visibility.Collapsed;
                lblWebTitle.Visibility = Visibility.Collapsed;
                return;
            }
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
        }

        private void LoadEmail()
        {
            string emailAddress = eapConfig.InstitutionInfo.EmailAddress;
            if (string.IsNullOrEmpty(emailAddress))
            {
                tbEmailText.Visibility = Visibility.Collapsed;
                lblEmailTitle.Visibility = Visibility.Collapsed;
                return;
            }
            bool isValidEmail = !(emailAddress.Contains(" ") || !emailAddress.Contains("@"));
            // show url as link
            if (isValidEmail)
            {
                tbEmailLink.Text = emailAddress;
                hlinkEmail.NavigateUri = new Uri("mailto:" + emailAddress);
                hlinkEmail.TextDecorations = null;
            }
            // show url but not as link
            else
            {
                tbEmailText.Text = emailAddress;
            }
        }

        private void LoadPhone()
        {
            string phone = eapConfig.InstitutionInfo.Phone;
            if (string.IsNullOrEmpty(phone))
            {
                tbPhoneText.Visibility = Visibility.Collapsed;
                lblPhoneTitle.Visibility = Visibility.Collapsed;
                return;
            }
            tbPhoneText.Text = phone;
        }

       
        private static bool HasContactInfo(EapConfig.ProviderInfo info)
        {
            bool hasWebAddress = !string.IsNullOrEmpty(info.WebAddress);
            bool hasEmailAddress = !string.IsNullOrEmpty(info.EmailAddress);
            bool hasPhone = !string.IsNullOrEmpty(info.Phone);
            return (hasWebAddress || hasEmailAddress || hasPhone);
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
                // TODO ...what to do here?
            }
           
        }

        private void LoadAlternate()
        {
            if (extractFlag)
            {
                mainWindow.btnBack.Visibility = Visibility.Hidden;
                gridAlt.Visibility = Visibility.Visible;
                tbAlt.Text = $"Not affiliated with {eapConfig.InstitutionInfo.DisplayName}?";
            }
            else
            {
                gridAlt.Visibility = Visibility.Collapsed;
            }
        }

        private void LinkClick(object sender, RequestNavigateEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }
        private void Hyperlink_TOU(object sender, RequestNavigateEventArgs e)
        {
            ShowTou = true;
            mainWindow.NextPage();
            e.Handled = true;
        }

        private void btnAlt_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.LoadPageMainMenu();
        }
        private static void OpenBrowser(string url)
		{
            System.Diagnostics.Process.Start(url);
        }
        private static void CopyToClipboard(string url)
		{
            Clipboard.SetText(url);
		}
		private void MenuItem_Click_OpenLink(object sender, RoutedEventArgs e)
		{
            OpenBrowser(eapConfig.InstitutionInfo.WebAddress);
        }

        private void MenuItem_Click_CopyLink(object sender, RoutedEventArgs e)
		{
            CopyToClipboard(eapConfig.InstitutionInfo.WebAddress);
        }
        private void MenuItem_Click_OpenEMail(object sender, RoutedEventArgs e)
        {
            OpenBrowser("mailto:" + Uri.EscapeDataString(eapConfig.InstitutionInfo.EmailAddress));
        }

        private void MenuItem_Click_CopyEMail(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(eapConfig.InstitutionInfo.EmailAddress);
        }

        private void MenuItem_Click_CopyPhone(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(eapConfig.InstitutionInfo.Phone);
        }
    }
}
