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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using EduroamConfigure;
using System.Diagnostics;
using System.Globalization;

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for InstalledProfile.xaml
	/// </summary>
	public partial class InstalledProfile : Page
	{
		private MainWindow mainWindow;
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

		private void LinkClick(object sender, RequestNavigateEventArgs e)
		{
			Hyperlink hl = (Hyperlink)sender;
			string navigateUri = hl.NavigateUri.ToString();
			Process.Start(new ProcessStartInfo(navigateUri));
			e.Handled = true;
		}

		private void LoadCertInfo()
		{
			var cert = PersistingStore.InstalledCertificates.FirstOrDefault();
			tbExpires.Text = cert.NotAfter.ToString(CultureInfo.InvariantCulture);

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
	}


}
