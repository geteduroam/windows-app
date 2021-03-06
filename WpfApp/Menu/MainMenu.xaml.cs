using EduroamConfigure;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for MainMenu.xaml
	/// </summary>
	public partial class MainMenu : Page
	{
		private readonly MainWindow mainWindow;
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

			// Pre-loading when starting the application makes things a bit faster,
			// but is it also acceptable from a privacy perspective?
			Task.Run(() => mainWindow.IdpDownloader.LoadProviders(useGeodata: true));
			if (!tbInstalledProfile.IsVisible && !btnExisting.IsVisible)
				tbNewProfile.Focus();
		}
		/// <summary>
		/// If no providers available try to download them
		/// </summary>
		private async Task<bool> LoadProviders()
		{
			bool online = false;
			// disable the discovery button until we are done
			btnNewProfile.IsEnabled = false;
			tbNewProfile.Text = "Loading ...";
			try
			{
				await mainWindow.IdpDownloader.LoadProviders(useGeodata: true);
				online = mainWindow.IdpDownloader.Loaded;
				tbNewProfile.Text = "Connect to eduroam";
			}
			catch (ApiParsingException e)
			{
				// Must never happen, because if the discovery is reached,
				// it must be parseable. Logging has been done upstream.
				tbNewProfile.Text = "API error";
				MessageBox.Show(e.Message, e.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (ApiUnreachableException)
			{
				tbNewProfile.Text = "No internet connection";
			}
			btnNewProfile.IsEnabled = true;

			return online;
		}

		private async void btnNewProfile_Click(object sender, RoutedEventArgs e)
		{
			if (!mainWindow.IdpDownloader.LoadedWithGeo)
			{
				await LoadProviders();
			}

			// The value may have changed, so check again
			if (mainWindow.IdpDownloader.Loaded)
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

		private void btnInstalledProfile_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.LoadPageInstalledProfile();
		}

		private void Page_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			foreach (FrameworkElement fwe in new FrameworkElement[] { tbInstalledProfile, btnExisting, btnNewProfile })
			{
				if (fwe.IsVisible && fwe.IsEnabled)
				{
					fwe.Focus();
					return;
				}
			}
		}
	}
}
