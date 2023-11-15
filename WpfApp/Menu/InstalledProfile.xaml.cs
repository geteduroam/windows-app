using EduroamConfigure;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for InstalledProfile.xaml
	/// </summary>
	public partial class InstalledProfile : Page
	{
		private readonly MainWindow mainWindow;
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
		/// Loads info regarding the certificate of the persising store and displays it to the usr
		/// </summary>
		public void LoadCertInfo()
		{
			if (PersistingStore.IdentityProvider.Value.NotAfter != null)
			{
				var expireDate = PersistingStore.IdentityProvider.Value.NotAfter;
				var nowDate = DateTime.Now;
				var diffDate = expireDate - nowDate;
				tbConnectedTo.Text = "Your account is valid for";
				tbTimeLeft.ToolTip = expireDate?.ToString(CultureInfo.InvariantCulture);

				if (diffDate.Value.Days > 1)
				{
					tbTimeLeft.Text = diffDate.Value.Days.ToString(CultureInfo.InvariantCulture) + " more days";
				}
				else if (diffDate.Value.Hours > 1)
				{
					tbTimeLeft.Text = diffDate.Value.Hours.ToString(CultureInfo.InvariantCulture) + " more hours";
				}
				else
				{
					tbTimeLeft.Text = diffDate.Value.Minutes.ToString(CultureInfo.InvariantCulture) + " more minutes";
				}
			}
			else
			{
				stpCert.Visibility = Visibility.Collapsed;
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
			mainWindow.NextPage();
		}

	}

}
