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
using System.Xml;
using WpfApp.Menu;
using EduroamConfigure;

namespace WpfApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		// TODO: Make Title contain more words / wrap around
		private enum FormId
		{
			MainMenu,
			SelectInstitution,
			SelectProfile,
			ProfileOverview,
			Download,
			Login,
			Connect,
			Redirect,
			SaveAndQuit,
			Loading,
			CertificateOverview,
		}

		public enum ProfileStatus
		{
			NoneConfigured,
			Incomplete,
			Working,
		}

		private readonly List<FormId> historyFormId = new List<FormId>();
		private FormId currentFormId;
		private MainMenu pageMainMenu;
		private SelectInstitution pageSelectInstitution;
		private SelectProfile pageSelectProfile;
		private ProfileOverview pageProfileOverview;
		private Loading pageLoading;
		private Login pageLogin;
		private CertificateOverview pageCertificateOverview;
		private Redirect pageRedirect;
		private bool Online;
		private EapConfig eapConfig;
		public ProfileStatus ProfileCondition { get; set; }
		public IdentityProviderDownloader IdpDownloader;
		public bool EduroamAvailable { get; set; }
		public EapConfig.AuthenticationMethod AuthMethod { get; set; }
		public MainWindow()
		{
			InitializeComponent();
			try
			{
				IdpDownloader = new IdentityProviderDownloader();
				Online = true;
			}
			catch (ApiException)
			{
				Online = false;
			}

			LoadPageMainMenu();
		}

		public void Navigate(Page nextPage)
		{
			Main.Content = nextPage;
		}

		public async void NextPage()
		{
			// adds current form to history for easy backtracking
			historyFormId.Add(currentFormId);
			switch (currentFormId)
			{
				case FormId.MainMenu:
					LoadPageSelectInstitution();
					break;

				case FormId.SelectInstitution:
					var profiles = GetProfiles((int)pageSelectInstitution.IdProviderId);
					if (profiles.Count == 1)
					{
						string autoProfileId = profiles.FirstOrDefault().Id;
						if (!string.IsNullOrEmpty(autoProfileId))
						{
							// if profile could not be handled then return to form
							if(! await HandleProfileSelect(autoProfileId)) LoadPageSelectInstitution(refresh: false);
							break;
						}
					}
					LoadPageSelectProfile();
					break;

				case FormId.SelectProfile:
					string profileId = pageSelectProfile.ProfileId;
					// if profile could not be handled then return to form
					if (!await HandleProfileSelect(profileId)) LoadPageSelectProfile(refresh: false);
					break;

				case FormId.ProfileOverview:
					LoadPageCertificateOverview();
					break;

				case FormId.CertificateOverview:
					LoadPageLogin();
					break;

				case FormId.Login:
					break;
				case FormId.Redirect:
					break;
			}


			// removes current form from history if it gets added twice
			if (historyFormId.LastOrDefault() == currentFormId) historyFormId.RemoveAt(historyFormId.Count - 1);

		}

		public void PreviousPage()
		{
			// clears logo if going back from summary page
			//if (currentFormId == FormId.Summary) ResetLogo();
			switch (historyFormId.Last())
			{
				case FormId.MainMenu:
					LoadPageMainMenu();
					break;
				case FormId.SelectInstitution:
					LoadPageSelectInstitution();
					break;
				case FormId.SelectProfile:
					LoadPageSelectProfile();
					break;
				case FormId.ProfileOverview:
					LoadPageProfileOverview();
					break;
				case FormId.CertificateOverview:
					LoadPageCertificateOverview();
					break;
				case FormId.Login:
					LoadPageLogin();
					break;
			}

			// removes current form from history
			historyFormId.RemoveAt(historyFormId.Count - 1);
		}

		// downloads eap config based on profileId
		// seperated into its own function as this can happen either through
		// user selecting a profile or a profile being autoselected
		private async Task<bool> HandleProfileSelect(string profileId)
		{
			LoadPageLoading();
			IdentityProviderProfile profile = IdpDownloader.GetProfileFromId(profileId);
			try
			{
				eapConfig = await DownloadEapConfig(profile);
			}
			catch (EduroamAppUserError ex) // TODO: register this in some higher level
			{
				MessageBox.Show(
					ex.UserFacingMessage,
					"eduroam - Exception");
				eapConfig = null;

			}

			// reenable buttons after LoadPageLoading() disables them
			btnBack.IsEnabled = true;
			btnNext.IsEnabled = true;

			if (eapConfig != null)
			{
				if(HasInfo(eapConfig))
				{
					 LoadPageProfileOverview();
					 return true;
				}
				LoadPageCertificateOverview();
				return true;
			}
			else if (!string.IsNullOrEmpty(profile.redirect))
			{
				// TODO: add option to go to selectmethod from redirect
				LoadPageRedirect(profile.redirect);
				return true;
			}
			return false;
		}

		private bool HasInfo(EapConfig config)
		{
			bool hasWebAddress = !string.IsNullOrEmpty(config.InstitutionInfo.WebAddress);
			bool hasEmailAddress = !string.IsNullOrEmpty(config.InstitutionInfo.EmailAddress);
			bool hasDescription = !string.IsNullOrEmpty(config.InstitutionInfo.Description);
			bool hasPhone = !string.IsNullOrEmpty(config.InstitutionInfo.Phone);
			bool hasTou = !string.IsNullOrEmpty(config.InstitutionInfo.TermsOfUse);
			return (hasWebAddress || hasEmailAddress || hasDescription || hasPhone || hasTou);
		}

		/// <summary>
		/// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
		/// </summary>
		private List<IdentityProviderProfile> GetProfiles(int providerId)
		{
			return IdpDownloader.GetIdentityProviderProfiles(providerId);

		}

		/// <summary>
		/// Gets EAP-config file, either directly or after browser authentication.
		/// Prepares for redirect if no EAP-config.
		/// </summary>
		/// <returns>EapConfig object.</returns>
		/// <exception cref="EduroamAppUserError">description</exception>
		public async Task<EapConfig> DownloadEapConfig(IdentityProviderProfile profile)
		{
			if (string.IsNullOrEmpty(profile?.Id))
				return null;

			EapConfig eapConfig; // return value

			// if OAuth
			if (profile.oauth)
			{
				// get eap config file from browser authenticate

				OAuth oauth = new OAuth(new Uri(profile.authorization_endpoint));
				// The url to send the user to
				var authUri = oauth.CreateAuthUri();
				// The url to listen to for the user to be redirected back to
				var prefix = oauth.GetRedirectUri();

				// Send the user to the url and await the response
				var responseUrl = OpenSSOAndAwaitResultRedirect(prefix.ToString(), authUri.ToString());

				// Parse the result and download the eap config if successfull
				(string authorizationCode, string codeVerifier) = oauth.ParseAndExtractAuthorizationCode(responseUrl);
				bool success = LetsWifi.RequestAccess(profile, authorizationCode, codeVerifier, prefix);

				eapConfig = success
					? LetsWifi.DownloadEapConfig()
					: null;
				// return focus to application
				Activate();
			}
			else if (!string.IsNullOrEmpty(profile.redirect))
			{
				//TODO handle redirect
				// makes redirect link accessible in parent form
				//RedirectUrl = redirect;
				return null;
			}
			else
			{
				eapConfig = await Task.Run(() =>
					IdpDownloader.DownloadEapConfig(profile.Id)
				);

			}
			return eapConfig;
		}

		/// <summary>
		/// Tries to connect to eduroam
		/// </summary>
		/// <returns></returns>
		public async Task<bool> Connect()
		{
			bool connectSuccess;
			try
			{
				connectSuccess = await Task.Run(ConnectToEduroam.TryToConnect);
			}
			catch (Exception ex)
			{
				connectSuccess = false;
				MessageBox.Show("Could not connect. \nException: " + ex.Message);
			}
			return connectSuccess;
		}


		// TODO: make new responesurl thing to receive

		/// <summary>
		/// Gets a response URL after doing Browser authentication with Oauth authUri.
		/// </summary>
		/// <returns>response Url as string.</returns>
		public string OpenSSOAndAwaitResultRedirect(string redirectUri, string authUri)
		{
			/*
			using var waitForm = new frmWaitDialog(redirectUri, authUri);
			DialogResult result = waitForm.ShowDialog();
			if (result != DialogResult.OK)
			{
				return "";
			}
			return waitForm.responseUrl;  //= WebServer.NonblockingListener(redirectUri, authUri, parentLocation);
			*/
			return "";
		}

		public void LoadPageMainMenu(bool refresh = true)
		{
			currentFormId = FormId.MainMenu;
			btnNext.Visibility = Visibility.Hidden;
			btnBack.Visibility = Visibility.Hidden;
			if (refresh) pageMainMenu = new MainMenu(this);
			Navigate(pageMainMenu);
		}

		public void LoadPageSelectInstitution(bool refresh = true)
		{
			currentFormId = FormId.SelectInstitution;
			btnNext.Visibility = Visibility.Visible;
			btnNext.Content = "Next >";
			btnBack.IsEnabled = true;
			btnBack.Visibility = Visibility.Visible;
			if (refresh) pageSelectInstitution = new SelectInstitution(this);

			Navigate(pageSelectInstitution);
		}

		public void LoadPageSelectProfile(bool refresh = true)
		{
			currentFormId = FormId.SelectProfile;
			btnNext.Visibility = Visibility.Visible;
			if (refresh) pageSelectProfile = new SelectProfile(this, pageSelectInstitution.IdProviderId);
			Navigate(pageSelectProfile);
		}

		public void LoadPageProfileOverview(bool refresh = true)
		{
			currentFormId = FormId.ProfileOverview;
			btnNext.Visibility = Visibility.Visible;
			btnNext.IsEnabled = true;
			btnBack.Visibility = Visibility.Visible;
			btnNext.Content = eapConfig.AuthenticationMethods.First().EapType == EapType.TLS ? "Connect" : "Next";
			if (refresh) pageProfileOverview = new ProfileOverview(this, eapConfig);
			Navigate(pageProfileOverview);
		}

		public void LoadPageCertificateOverview(bool refresh = true)
		{
			// if all certificates are installed then skip to login
			currentFormId = FormId.CertificateOverview;
			btnBack.Visibility = Visibility.Visible;
			btnBack.IsEnabled = true;
			if (refresh) pageCertificateOverview = new CertificateOverview(this, eapConfig);
			Navigate(pageCertificateOverview);
		}

		public void LoadPageLogin(bool refresh = true)
		{
			currentFormId = FormId.Login;
			if (refresh) pageLogin = new Login(this);
			Navigate(pageLogin);
		}

		public void LoadPageRedirect(string redirect, bool refresh = true)
		{
			currentFormId = FormId.Redirect;
			btnBack.IsEnabled = true;
			btnNext.IsEnabled = false;
			if (refresh) pageRedirect = new Redirect(this, redirect);
			Navigate(pageRedirect);
		}

		public void LoadPageLoading(bool refresh = true)
		{
			currentFormId = FormId.Loading;
			btnBack.IsEnabled = false;
			btnNext.IsEnabled = false;
			if (refresh) pageLoading = new Loading(this);
			Navigate(pageLoading);
		}


		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			NextPage();
		}

		private void btnBack_Click(object sender, RoutedEventArgs e)
		{
			PreviousPage();
		}
	}
}
