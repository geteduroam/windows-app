using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EduroamConfigure;

namespace WpfApp.Menu
{
    /// <summary>
    /// Interaction logic for SelectProfile.xaml
    /// </summary>
    public partial class SelectProfile : Page
    {
        private readonly MainWindow mainWindow; // makes parent form accessible from this class
        private readonly int IdProviderId; // id of selected institution
        public string ProfileId { get; set; } // id of selected institution profile

        public SelectProfile(MainWindow mainWindow, int providerId)
        {
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
            this.IdProviderId = providerId;
            InitializeComponent();
            LoadPage();
        }

        private async void LoadPage()
        {
            tbTitle.Text = "Select profile";
            mainWindow.btnNext.IsEnabled = false;


            lbProfiles.IsEnabled = false;

            await Task.Run(() => PopulateProfiles());

            lbProfiles.IsEnabled = true;



        }

        /// <summary>
        /// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
        /// </summary>
        private void PopulateProfiles()
        {
            var idProviderProfiles = mainWindow.IdpDownloader.GetIdentityProviderProfiles(IdProviderId);
            this.Dispatcher.Invoke(() => {
                lbProfiles.ItemsSource = idProviderProfiles;
                lbProfiles.SelectedItem = idProviderProfiles.First();
            });
        }

        /// <summary>
        /// Called when user selects a profile
        /// </summary>
        private void lbProfiles_SelectionChanged(object sender, EventArgs e)
        {
            // if user clicks on empty area of the listbox it will cause event but no item is selected
            if (lbProfiles.SelectedItem == null) return;

            // gets id of selected profile
            var selectedProfile = (IdentityProviderProfile) lbProfiles.SelectedItem;
            ProfileId = selectedProfile.Id;
            mainWindow.btnNext.IsEnabled = true;
        }

        private void lbProfiles_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (lbProfiles.SelectedItem != null)
            {
                mainWindow.NextPage();
            }           
        }

        private void Page_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)lbProfiles.ItemContainerGenerator.ContainerFromIndex(0);
            FocusManager.SetFocusedElement(this, item);
        }
    }
}
