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
using System.IO;
using System.Reflection;
using WpfApp.Menu;
using EduroamConfigure;
using System.Diagnostics;
using System.ComponentModel;
using Hardcodet.Wpf.TaskbarNotification;

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
			TermsOfUse,
			OAuthWait
		}

		public enum ProfileStatus
		{
			NoneConfigured,
			Configured,
			TestedWorking,
		}

		private readonly List<FormId> historyFormId = new List<FormId>();
		private FormId currentFormId;
		private MainMenu pageMainMenu;
		private SelectInstitution pageSelectInstitution;
		private SelectProfile pageSelectProfile;
		private ProfileOverview pageProfileOverview;
		private Loading pageLoading;
		private Login pageLogin;
		private TermsOfUse pageTermsOfUse;
		private CertificateOverview pageCertificateOverview;
		private Redirect pageRedirect;
		private OAuthWait pageOAuthWait;
		// this contains the 'active' eapConfig that is being used
		private EapConfig eapConfig;
		// If theres is a bundled config file then it is stored in this variable
		public EapConfig ExtractedEapConfig { get; set; }
		//ExtractFlag decides if the "Not affiliated with this institution? choose another one" text and button shows up on ProfileOverview or not
		public bool ExtractFlag { get; set; }

		public ProfileStatus ProfileCondition { get; set; }
		public IdentityProviderDownloader IdpDownloader { get; private set; }
		public bool EduroamAvailable { get; set; }
		public MainWindow()
		{
			WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
			InitializeComponent();
			Load();
			if (App.StartHiddenInTray) Hide();
		}

		private void Load()
		{
			IdpDownloader = new IdentityProviderDownloader();

			ExtractedEapConfig = GetSelfExtractingEap();
			if (ExtractedEapConfig != null)
			{
				// sets flags
				//ComesFromSelfExtract = true;
				//SelfExtractFlag = true;
				// reset web logo or else it won't load
				//ResetLogo();
				// loads summary form so user can confirm installation
				eapConfig = ExtractedEapConfig;
				ExtractFlag = true;
				LoadPageProfileOverview();
			}
			else
			{
				LoadPageMainMenu();
			}
		}

		public void Navigate(Page nextPage)
		{
			// if nothing to go back to, hide back button

			Main.Content = nextPage;
			ValidateBackButton();
		}

		public async void NextPage()
		{
			// adds current form to history for easy backtracking
			historyFormId.Add(currentFormId);
			switch (currentFormId)
			{
				case FormId.MainMenu:
					if (pageMainMenu.LocalEapConfig != null)
					{
						eapConfig = pageMainMenu.LocalEapConfig;
						LoadPageProfileOverview();
						break;
					}
					if (pageMainMenu.UseExtracted)
					{
						eapConfig = ExtractedEapConfig;
						LoadPageProfileOverview();
						break;
					}

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
					if(pageProfileOverview.ShowTou)
					{
						LoadPageTermsOfUse();
						break;
					}
					LoadPageCertificateOverview();
					break;
				case FormId.TermsOfUse:
					historyFormId.Remove(currentFormId);
					PreviousPage();
					break;

				case FormId.CertificateOverview:
					LoadPageLogin();
					break;

				case FormId.Login:
					if(pageLogin.IsConnected)
					{
						Shutdown();
						break;
					}
					pageLogin.ConnectClick();
					break;
				case FormId.Redirect:
					break;
			}


			// removes current form from history if it gets added twice
			if (historyFormId.LastOrDefault() == currentFormId) historyFormId.RemoveAt(historyFormId.Count - 1);

			ValidateBackButton();

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
					if (currentFormId == FormId.OAuthWait) pageOAuthWait.CancelThread();
					LoadPageSelectInstitution();
					break;
				case FormId.SelectProfile:
					if (currentFormId == FormId.OAuthWait) pageOAuthWait.CancelThread();
					LoadPageSelectProfile();
					break;
				case FormId.ProfileOverview:
					LoadPageProfileOverview();
					break;
				case FormId.CertificateOverview:
					if (Main.Content == pageLogin)
					{
						pageLogin.IgnorePasswordChange = true;
						pageLogin.dispatcherTimer.Stop();
					}
					LoadPageCertificateOverview();
					break;
				case FormId.Login:
					LoadPageLogin();
					break;

			}

			// removes current form from history
			historyFormId.RemoveAt(historyFormId.Count - 1);

			ValidateBackButton();
		}
		/// <summary>
		/// Hide back button if theres no page to go back to
		/// </summary>
		private void ValidateBackButton()
		{
			if (historyFormId.Count < 1) btnBack.Visibility = Visibility.Hidden;
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
				LoadPageRedirect(new Uri(profile.redirect));
				return true;
			}
			else if (profile.oauth)
			{

				LoadPageOAuthWait(profile);
				return true;
			}
			return false;
		}

		private static bool HasInfo(EapConfig config)
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
			if (profile.oauth || !string.IsNullOrEmpty(profile.redirect))
			{
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
		public static async Task<bool> Connect()
		{
			bool connectSuccess;
			try
			{
				connectSuccess = await Task.Run(ConnectToEduroam.TryToConnect);
			}
			catch (Exception ex)
			{
				// NICE TO HAVE: log the error
				connectSuccess = false;
				MessageBox.Show("Could not connect. \nException: " + ex.Message);
			}
			return connectSuccess;
		}

		/// <summary>
		/// Checks if an EAP-config file exists in the same folder as the executable
		/// </summary>
		/// <returns>EapConfig object if file exists, null if not.</returns>
		public EapConfig GetSelfExtractingEap()
		{
			string exeLocation = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string[] files = Directory.GetFiles(exeLocation, "*.eap-config");

			if (files.Length <= 0) return null;
			try
			{
				string eapPath = files.First(); // TODO: although correct, this seems smelly
				string eapString = File.ReadAllText(eapPath);
				//eapConfig = EapConfig.FromXmlData(uid: "bundled file", eapString);
				return EapConfig.FromXmlData(uid: "bundled file", eapString);
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static App App { get => (App)Application.Current; }


		public void OAuthComplete(EapConfig eapConfig)
		{
			Activate();
			this.eapConfig = eapConfig;
			if (eapConfig != null)
			{
				if (HasInfo(eapConfig))
				{
					LoadPageProfileOverview();
				}
				LoadPageCertificateOverview();
			}
			else
			{
				PreviousPage();
			}
		}

		public void LoadPageMainMenu(bool refresh = true)
		{
			ExtractFlag = false;
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
			btnNext.Content = "Next";
			btnBack.IsEnabled = true;
			btnBack.Visibility = Visibility.Visible;
			if (refresh) pageSelectInstitution = new SelectInstitution(this);

			Navigate(pageSelectInstitution);
		}

		public void LoadPageSelectProfile(bool refresh = true)
		{
			currentFormId = FormId.SelectProfile;
			btnNext.Visibility = Visibility.Visible;
			btnNext.Content = "Next";
			if (refresh) pageSelectProfile = new SelectProfile(this, pageSelectInstitution.IdProviderId);
			Navigate(pageSelectProfile);
		}

		public void LoadPageProfileOverview(bool refresh = true)
		{
			currentFormId = FormId.ProfileOverview;
			btnNext.Visibility = Visibility.Visible;
			btnNext.IsEnabled = true;
			btnNext.Content = "Next";
			btnBack.Visibility = Visibility.Visible;
			if (refresh) pageProfileOverview = new ProfileOverview(this, eapConfig);
			Navigate(pageProfileOverview);
		}

		public void LoadPageCertificateOverview(bool refresh = true)
		{
			// if all certificates are installed then skip to login
			currentFormId = FormId.CertificateOverview;
			btnBack.Visibility = Visibility.Visible;
			btnBack.IsEnabled = true;
			btnNext.Content = "Next";
			if (refresh) pageCertificateOverview = new CertificateOverview(this, eapConfig);
			Navigate(pageCertificateOverview);
		}

		public void LoadPageLogin(bool refresh = true)
		{
			currentFormId = FormId.Login;
			if (refresh) pageLogin = new Login(this, eapConfig);
			Navigate(pageLogin);
		}

		public void LoadPageRedirect(Uri redirect, bool refresh = true)
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


		public void LoadPageTermsOfUse(bool refresh = true)
		{
			currentFormId = FormId.TermsOfUse;
			btnBack.IsEnabled = true;
			btnNext.Content = "OK";
			btnNext.Visibility = Visibility.Visible;
			if (refresh) pageTermsOfUse = new TermsOfUse(this, eapConfig.InstitutionInfo.TermsOfUse);
			Navigate(pageTermsOfUse);
		}

		public void LoadPageOAuthWait(IdentityProviderProfile profile)
		{
			currentFormId = FormId.OAuthWait;
			btnBack.IsEnabled = true;
			btnNext.IsEnabled = false;
			pageOAuthWait = new OAuthWait(this, profile);
			Navigate(pageOAuthWait);
		}

		private bool IsShuttingDown = false;
		public void Shutdown(int exitCode = 0)
		{
			IsShuttingDown = true;
			Application.Current.Shutdown(exitCode);
		}


		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			NextPage();
		}

		private void btnBack_Click(object sender, RoutedEventArgs e)
		{
			PreviousPage();
		}

		// Logic to minimize to tray:

		private void OnWindowClose(object sender, CancelEventArgs e)
		{
			Debug.WriteLine("Event: OnClose");
			Debug.WriteLine("Sender: " + sender.ToString());
			Debug.WriteLine("{0}: {1}", nameof(IsShuttingDown), IsShuttingDown);
			Debug.WriteLine("{0}: {1}", nameof(App.Installer.IsInstalled), App.Installer.IsInstalled);
			Debug.WriteLine("{0}: {1}", nameof(App.Installer.IsRunningFromInstallLocation), App.Installer.IsRunningFromInstallLocation);

			if (!App.Installer.IsInstalled)
				return; // do not cancel the Closing event

			if (App.Installer.IsInstalled && !App.Installer.IsRunningFromInstallLocation)
			{
				// this happens after the first time setup
				SelfInstaller.DelayedStart(App.Installer.StartMinimizedCommand);
				return; // do not cancel the Closing event
			}

			if (IsShuttingDown)
				return; // closed in tray icon, unable to cancel. avoid creating the balloon

			// Cancels the Window.Close(), but unable to cancel Application.Shutdown()
			e.Cancel = true;

			TaskbarIcon.ShowBalloonTip( // TODO: doesn't show for peder, but does show for simon. RDP might be the culprit
				title: "geteduroam",
				message: "geteduroam is still running in the background",
				symbol: BalloonIcon.Info);

			Hide(); // window
		}

		private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
			=> tb_TrayLeftMouseDown(sender, e);

		private void tb_TrayLeftMouseDown(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("Event: TrayLeftMouseDown");
			if (!IsVisible)
			{
				Show(); // window
				if (WindowState == WindowState.Minimized)
					WindowState = WindowState.Normal;
				Activate(); // focus window
			}
			else
			{
				Hide(); // window
			}
		}

		private void MenuItem_Click_Show(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("Event: MenuItem_Click_Show");
			Show(); // window
			if (WindowState == WindowState.Minimized)
				WindowState = WindowState.Normal;
			Activate(); // focus window
		}

		private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
			=> Shutdown();

	}
}
