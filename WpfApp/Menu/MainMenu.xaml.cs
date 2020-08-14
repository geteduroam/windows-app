using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EduroamConfigure;
using WpfApp.Classes;

namespace WpfApp.Menu
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        private readonly MainWindow mainWindow;
        public EapConfig LocalEapConfig { get; set; }
        public bool UseExtracted { get; set; }
        public MainMenu(MainWindow mainWindow)

        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            mainWindow.btnNext.Visibility = Visibility.Hidden;
            mainWindow.btnBack.Visibility = Visibility.Hidden;

            grdInstalledProfile.Visibility = Visibility.Collapsed;
            
            tbInfo.Visibility = Visibility.Hidden;

            if (PersistingStore.IdentityProvider != null)
            {
                tbInstalledProfile.Text = PersistingStore.IdentityProvider.Value.DisplayName;
                grdInstalledProfile.Visibility = Visibility.Visible;
            }

            if (mainWindow.ExtractedEapConfig == null)
            {
                btnExisting.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnExisting.Visibility = Visibility.Visible;
                tbExisting.Text = "Connect with " + mainWindow.ExtractedEapConfig.InstitutionInfo.DisplayName;
            }
            LoadProviders();
          

        }
        /// <summary>
        /// If no providers available try to download them
        /// </summary>
        private async void LoadProviders()
        {
            if (!mainWindow.IdpDownloader.Online)
            {
                try
                {
                    // this will make the window pop up a little bit faster and disable the discovery button until institutions are loaded
                    BtnNewProfile.IsEnabled = false;
                    tbNewProfile.Text = "Loading ...";
                    await Task.Run(() => mainWindow.IdpDownloader.LoadProviders());
                    BtnNewProfile.IsEnabled = true;
                    tbNewProfile.Text = "Connect to eduroam"; 
                }
                catch (ApiException)
                {
                    tbNewProfile.Text = "Discovery service down";
                    BtnNewProfile.IsEnabled = false;
                }
                catch (InternetConnectionException)
                {
                    tbNewProfile.Text = "No internet connection";
                    BtnNewProfile.IsEnabled = false;
                }
            }
        }

        private void btnNewProfile_Click(object sender, RoutedEventArgs e)
        {
            if (!mainWindow.IdpDownloader.Online)
            {
                LoadProviders();
            }
            else
            {
                mainWindow.NextPage();
            }
        }

        private void btnExisting_Click(object sender, RoutedEventArgs e)
        {
            UseExtracted = true;
            mainWindow.ExtractFlag = true;
            mainWindow.NextPage();
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            LocalEapConfig = FileDialog.AskUserForEapConfig();
            if (LocalEapConfig == null)
                LocalEapConfig = null;
            else if (!MainWindow.CheckIfEapConfigIsSupported(LocalEapConfig))
                LocalEapConfig = null;
            if (LocalEapConfig == null) return;
            mainWindow.NextPage();
        }

        private void btnInstalledProfile_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.LoadPageInstalledProfile();
        }
    }
}
