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

			LoadContactInfo();
			LoadTOU();
			LoadAlternate();

		}

		private void LoadContactInfo()
		{
			string webAddress = eapConfig.InstitutionInfo.WebAddress;
			string emailAddress = eapConfig.InstitutionInfo.EmailAddress;

			// displays website, email, phone number
			lblWeb.Content = webAddress != "" ? webAddress : "-";
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

	}
}
