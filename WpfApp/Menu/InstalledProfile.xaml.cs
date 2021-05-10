using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Globalization;
using EduroamConfigure;
using System.Net.Http;

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for InstalledProfile.xaml
	/// </summary>
	public partial class InstalledProfile : Page
	{
		private readonly MainWindow mainWindow;
		public string ProfileId
		{ get => PersistingStore.IdentityProvider?.ProfileId; }
		public string ReinstallEapConfigXml
		{ get => PersistingStore.IdentityProvider?.EapConfigXml; }
		public string ReinstallUsername
		{ get => PersistingStore.Username; }
		public bool IsRefreshable
		{ get => PersistingStore.IsRefreshable; }
		public bool GoToMain { get; set; }
		private string webAddress;
		private string phone;
		private string emailAddress;
		public InstalledProfile(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;
			InitializeComponent();
			Load();
		}

		private void Load()
		{
			tbName.Text = PersistingStore.IdentityProvider.Value.DisplayName;
			LoadContactInfo();
			LoadCertInfo();
			LoadProfile();

		}
		/// <summary>
		/// Used for Institution Info links to websites / mail
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LinkClick(object sender, RequestNavigateEventArgs e)
		{
			Hyperlink hl = (Hyperlink)sender;
			string navigateUri = hl.NavigateUri.ToString();
			Process.Start(new ProcessStartInfo(navigateUri));
			e.Handled = true;
		}

		/// <summary>
		/// Loads information from PersistingStore
		/// </summary>
		private async void LoadProfile()
		{
			if (PersistingStore.IsReinstallable)
			{
				mainWindow.btnNext.IsEnabled = true;
				mainWindow.btnNext.Content = "Reconnect";
				return;
			}

			// check if profile id exists in discovery

			//mainWindow.btnNext.IsEnabled = false;
			var profileId = ProfileId;
			if (!string.IsNullOrEmpty(profileId))
			{
				try
				{
					mainWindow.btnNext.Content = "Loading ...";
					if (!mainWindow.IdpDownloader.Online) await Task.Run(() => mainWindow.IdpDownloader.LoadProviders());
				}
				catch (ApiUnreachableException)
				{
					mainWindow.btnNext.IsEnabled = false;
					mainWindow.btnNext.Content = "Offline";
					return;
				}
				catch (ApiParsingException e)
				{
					// Must never happen, because if the discovery is reached,
					// it must be parseable. If it happens anyway, SCREAM!

					Debug.Print(e.ToString());
					throw;
				}

				// TODO: this ^ Online check should be moved into the IdpDownloader
				var profile = await Task.Run(() => mainWindow.IdpDownloader.GetProfileFromId(profileId));
				if (profile != null)
				{
					mainWindow.btnNext.IsEnabled = true;
					mainWindow.btnNext.Content = "Reconnect";
					return;
				}
				else
				{
					mainWindow.btnNext.IsEnabled = false;
					mainWindow.btnNext.Content = "Can't reconnect";
					//btnMainMenu.Style = FindResource("BlueButtonStyle") as Style;
					return;
				}
			}

			mainWindow.btnNext.IsEnabled = false;
			mainWindow.btnNext.Content = "Can't reconnect";
			// TODO: getting here means that we never should have been in this Form anyway. Move on to MainMenu instead?
		}

		/// <summary>
		/// Loads info regarding the certficate of the persising store and displays it to the usr
		/// </summary>
		private void LoadCertInfo()
		{
			if (PersistingStore.IdentityProvider.Value.NotAfter != null)
			{
				var expireDate = PersistingStore.IdentityProvider.Value.NotAfter;
				var nowDate = DateTime.Now;
				var diffDate = expireDate - nowDate;
				tbExpires.Text = "Exp: " +  expireDate?.ToString(CultureInfo.InvariantCulture);

				if(diffDate.Value.Days > 0)
				{
					tbTimeLeft.Text = diffDate.Value.Days.ToString(CultureInfo.InvariantCulture) + " Days left";
				}
				else if (diffDate.Value.Hours > 0)
				{
					tbTimeLeft.Text = diffDate.Value.Hours.ToString(CultureInfo.InvariantCulture) + " Hours left";
				}
				else
				{
					tbTimeLeft.Text = diffDate.Value.Minutes.ToString(CultureInfo.InvariantCulture) + " Minutes left";
				}
				btnRefresh.Visibility = IsRefreshable ? Visibility.Visible : Visibility.Collapsed;

				btnRefresh.Content = "Refresh now";
				btnRefresh.IsEnabled = true;
			}
			else
			{
				grpCert.Visibility = Visibility.Collapsed;
			}


		}

		/// <summary>
		/// Loads contact info from persisingstore
		/// </summary>
		private void LoadContactInfo()
		{
			webAddress = PersistingStore.IdentityProvider.Value.WebAddress;
			phone = PersistingStore.IdentityProvider.Value.Phone;
			emailAddress = PersistingStore.IdentityProvider.Value.EmailAddress;

			if (!HasContactInfo())
			{
				grpInfo.Visibility = Visibility.Collapsed;
				return;
			}

			LoadWeb();
			LoadEmail();
			LoadPhone();
		}

		/// <summary>
		/// displays web address information
		/// </summary>
		private void LoadWeb()
		{
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
		/// <summary>
		/// Displays Email info
		/// </summary>
		private void LoadEmail()
		{
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
		/// <summary>
		/// displays phone info
		/// </summary>
		private void LoadPhone()
		{
			if (string.IsNullOrEmpty(phone))
			{
				tbPhoneText.Visibility = Visibility.Collapsed;
				lblPhoneTitle.Visibility = Visibility.Collapsed;
				return;
			}
			tbPhoneText.Text = phone;
		}

		/// <summary>
		///  Checks if there is any contact info
		/// </summary>
		/// <returns></returns>
		private bool HasContactInfo()
		{
			bool hasWebAddress = !string.IsNullOrEmpty(webAddress);
			bool hasEmailAddress = !string.IsNullOrEmpty(emailAddress);
			bool hasPhone = !string.IsNullOrEmpty(phone);
			return (hasWebAddress || hasEmailAddress || hasPhone);
		}

		/// <summary>
		/// Button for going to the page MainMenu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnMainMenu_Click(object sender, RoutedEventArgs e)
		{
			GoToMain = true;
			mainWindow.NextPage();
			//mainWindow.LoadPageMainMenu();
		}

		/// <summary>
		/// Clickable button for user to delete the installed profile
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void btnLogout_Click(object sender, RoutedEventArgs e)
		{
			btnLogout.Content = "Logging out ..";
			await Task.Run(() => Logout());
			mainWindow.LoadPageMainMenu();
		}

		/// <summary>
		/// Uninstalls the installed WLAN profile
		/// </summary>
		private void Logout()
		{
			LetsWifi.WipeTokens();
			ConnectToEduroam.RemoveAllWLANProfiles();
			CertificateStore.UninstallAllInstalledCertificates(ommitRootCa: true);
			PersistingStore.IdentityProvider = null;

			// TODO: remove root CAs aswell in some nice way
		}

		private async void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			btnRefresh.Content = "Refreshing ...";
			btnRefresh.IsEnabled = false;
			var response = LetsWifi.RefreshResponse.Failed;
			try
			{
				response = await Task.Run(() => LetsWifi.RefreshAndInstallEapConfig(force: true, onlyLetsWifi: true));
			}
			catch (ApiParsingException ex)
			{
				MessageBox.Show(ex.Message, "Unable to refresh", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (HttpRequestException ex)
			{
				mainWindow.NextPage();
				return;
			}
			switch (response)
			{
				case LetsWifi.RefreshResponse.Success:
				case LetsWifi.RefreshResponse.UpdatedEapXml: // Should never happen due to onlyLetsWifi=true
					LoadCertInfo();
					break;
				case LetsWifi.RefreshResponse.StillValid: // should never happend due to force=true
				case LetsWifi.RefreshResponse.AccessDenied:
				case LetsWifi.RefreshResponse.NewRootCaRequired:
				case LetsWifi.RefreshResponse.NotRefreshable:
				case LetsWifi.RefreshResponse.Failed:
					btnRefresh.Content = "Cant refresh";
					btnRefresh.IsEnabled = false;
					break;
			}
		}
	}


}
