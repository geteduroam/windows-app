using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Globalization;
using EduroamConfigure;

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for InstalledProfile.xaml
	/// </summary>
	public partial class InstalledProfile : Page
	{
		private MainWindow mainWindow;
		public string ProfileId
		{ get => PersistingStore.IdentityProvider?.ProfileId; }
		public string ReinstallEapConfigXml
		{ get => PersistingStore.IdentityProvider?.EapConfigXml; }
		public string ReinstallUsername
		{ get => PersistingStore.Username; }
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

		private void LinkClick(object sender, RequestNavigateEventArgs e)
		{
			Hyperlink hl = (Hyperlink)sender;
			string navigateUri = hl.NavigateUri.ToString();
			Process.Start(new ProcessStartInfo(navigateUri));
			e.Handled = true;
		}

		private async void LoadProfile()
		{
			if (PersistingStore.IsReinstallable)
			{
				mainWindow.btnNext.IsEnabled = true;
				mainWindow.btnNext.Content = "Reconnect";
				return;
			}

			// check if profile id exists in discovery

			mainWindow.btnNext.IsEnabled = false;
			var profileId = ProfileId;
			if (!string.IsNullOrEmpty(profileId))
			{
				mainWindow.btnNext.Content = "Loading ...";
				if (!mainWindow.IdpDownloader.Online) await Task.Run(() => mainWindow.IdpDownloader.LoadProviders());
				// TODO: this ^ Online check should be moved into the IdpDownloader
				var profile = await Task.Run(() => mainWindow.IdpDownloader.GetProfileFromId(profileId));
				if (profile != null)
				{
					mainWindow.btnNext.IsEnabled = true;
					mainWindow.btnNext.Content = "Reconnect";
				}
				else
				{
					mainWindow.btnNext.Content = "Cant reconnect";
					//btnMainMenu.Style = FindResource("BlueButtonStyle") as Style;
				}
			}
			else
			{
				mainWindow.btnNext.Visibility = Visibility.Hidden;
				// TODO: getting here means that we never should have been in this form anyway. Move on to MainMenu instead?
			}
		}

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
			}
			else
			{
				grpCert.Visibility = Visibility.Collapsed;
			}


		}

		private void LoadContactInfo()
		{
			webAddress = PersistingStore.IdentityProvider.Value.WebAddress;
			phone = PersistingStore.IdentityProvider.Value.Phone;
			emailAddress = PersistingStore.IdentityProvider.Value.EmailAddress;

			if (!hasContactInfo())
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
			if (string.IsNullOrEmpty(phone))
			{
				tbPhoneText.Visibility = Visibility.Collapsed;
				lblPhoneTitle.Visibility = Visibility.Collapsed;
				return;
			}
			tbPhoneText.Text = phone;
		}


		private bool hasContactInfo()
		{
			bool hasWebAddress = !string.IsNullOrEmpty(webAddress);
			bool hasEmailAddress = !string.IsNullOrEmpty(emailAddress);
			bool hasPhone = !string.IsNullOrEmpty(phone);
			return (hasWebAddress || hasEmailAddress || hasPhone);
		}

		private void btnMainMenu_Click(object sender, RoutedEventArgs e)
		{
			GoToMain = true;
			mainWindow.NextPage();
			//mainWindow.LoadPageMainMenu();
		}

		private async void btnLogout_Click(object sender, RoutedEventArgs e)
		{
			btnLogout.Content = "Logging out ..";
			await Task.Run(() => Logout());
			mainWindow.LoadPageMainMenu();
		}

		//todo check if uninstall success
		private void Logout()
		{
			EduroamConfigure.ConnectToEduroam.RemoveAllProfiles();
			EduroamConfigure.LetsWifi.WipeTokens();
			EduroamConfigure.PersistingStore.IdentityProvider = null;
		}
	}


}
