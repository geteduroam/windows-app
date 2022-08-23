using EduroamConfigure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using WpfApp.Classes;
using WpfApp.Menu;

namespace WpfApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

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
			OAuthWait,
			InstalledProfile,
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
		private InstalledProfile pageInstalledProfile;
		// this contains the 'active' eapConfig that is being used
		private EapConfig eapConfig;
		// If theres is a bundled config file then it is stored in this variable
		public EapConfig ExtractedEapConfig { get; set; }
		//ExtractFlag decides if the "Not affiliated with this institution? choose another one" text and button shows up on ProfileOverview or not
		public bool ExtractFlag { get; set; }
		public string PresetUsername { get; private set; }
		public EapConfig LocalEapConfig { get; private set; }

		public ProfileStatus ProfileCondition { get; set; } // TODO: use this to determine if we need to clean up after a failed setup, negated by App.Installer.IsInstalled
		public IdentityProviderDownloader IdpDownloader { get; private set; }
		public bool EduroamAvailable { get; set; }
		public MainWindow()
		{
			WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
			InitializeComponent();
			string versionNumber = LetsWifi.VersionNumber;
			if (versionNumber == null)
			{
#if DEBUG
				lblVersion.Content = "DEBUG";
#else
				lblVersion.Content = "VERSIONLESS";
#endif
			}
			else
			{
#if DEBUG
				lblVersion.Content = LetsWifi.VersionNumber + "+DEBUG";
#else
				lblVersion.Content = LetsWifi.VersionNumber;
#endif
			}

			Load();
		}

		private void Load()
		{
			IdpDownloader = new IdentityProviderDownloader();
			ExtractedEapConfig = GetBundledEapConfig();
			if (PersistingStore.IdentityProvider != null)
			{
				LoadPageInstalledProfile();
			}
			else if (ExtractedEapConfig != null)
			{
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
			UpdateBackButton();
		}

		/// <summary>
		/// Logic for navigating to forward
		/// </summary>
		/// <exception cref="XmlException">TODO catch</exception>
		public async void NextPage()
		{
			// adds current form to history for easy backtracking
			historyFormId.Add(currentFormId);
			switch (currentFormId)
			{
				case FormId.InstalledProfile:
					LoadPageMainMenu();
					break;

				case FormId.MainMenu:
					if (LocalEapConfig != null)
					{
						eapConfig = LocalEapConfig;
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
					var profiles = IdpDownloader.GetIdentityProviderProfiles(pageSelectInstitution.IdProviderId);
					if (profiles.Count == 1) // skip the profile select and go with the first one
					{
						string autoProfileId = profiles.FirstOrDefault().Id;
						if (!string.IsNullOrEmpty(autoProfileId))
						{
							// if profile could not be handled then return to form
							if (!await HandleProfileSelect(autoProfileId)) LoadPageSelectInstitution(refresh: false);
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
					if (pageProfileOverview.ShowTou)
					{
						LoadPageTermsOfUse();
						break;
					}
					if (ConnectToEduroam.EnumerateCAInstallers(eapConfig)
						.Any(installer => installer.IsInstalledByUs || !installer.IsInstalled))
					{
						LoadPageCertificateOverview();
						break;
					}

					LoadPageLogin();
					break;
				case FormId.TermsOfUse:
					historyFormId.Remove(currentFormId);
					PreviousPage();
					break;

				case FormId.CertificateOverview:
					LoadPageLogin();
					break;

				case FormId.Login:
					if (pageLogin.IsConnected)
					{
						if (!App.Installer.IsRunningInInstallLocation)
						{
							Shutdown();
						}
						else
						{
							Hide();
							LoadPageInstalledProfile();
							historyFormId.Clear();
						}
						break;
					}
					pageLogin.ConnectClick();
					break;
				case FormId.Redirect:
					break;
			}


			// removes current form from history if it gets added twice
			if (historyFormId.LastOrDefault() == currentFormId) historyFormId.RemoveAt(historyFormId.Count - 1);

			UpdateBackButton();

		}
		/// <summary>
		/// Logic for navigating backwards
		/// </summary>
		public void PreviousPage()
		{
			if (historyFormId.Count == 0) return;
			if (currentFormId == FormId.Login)
			{
				pageLogin.IgnorePasswordChange = true;
				pageLogin.dispatcherTimer.Stop();
			}
			switch (historyFormId.Last())
			{
				case FormId.InstalledProfile:
					try
					{
						LoadPageInstalledProfile();
					}
					catch (InvalidOperationException)
					{
						// Do not crash, but still remove this item
						// This can happen when logging out and during logout doing to the main menu,
						// and then triggering a back operation, which would go back to the installed profile page,
						// but it cannot load anymore since you've logged out
					}
					break;
				case FormId.MainMenu:
					LoadPageMainMenu();
					break;
				case FormId.SelectInstitution:
					// shut down http server
					if (currentFormId == FormId.OAuthWait) OAuthWait.CancelThread();
					LoadPageSelectInstitution();
					break;
				case FormId.SelectProfile:
					// shut down http server
					if (currentFormId == FormId.OAuthWait) OAuthWait.CancelThread();
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

			UpdateBackButton();
		}
		/// <summary>
		/// Hide back button if theres no page to go back to
		/// </summary>
		private void UpdateBackButton()
		{
			if (historyFormId.Count < 1)
				btnBack.Visibility = Visibility.Hidden;
			btnBack.Style = (Style)App.Resources["HeaderButtonStyle"];
		}

		public static bool CheckIfEapConfigIsSupported(EapConfig eapConfig)
		{
			if (!EduroamNetwork.IsEapConfigSupported(eapConfig))
			{
				MessageBox.Show(
					"The profile you have selected is not supported by this application.",
					"No supported authentification method was found.",
					MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return false;
			}
			return true;
		}

		/// <summary>
		/// downloads eap config based on profileId
		/// seperated into its own function as this can happen either through
		/// user selecting a profile or a profile being autoselected
		/// </summary>
		/// <param name="profileId"></param>
		/// <param name="eapConfigXml"></param>
		/// <param name="skipOverview"></param>
		/// <returns>True if function navigated somewhere</returns>
		/// <exception cref="XmlException">Parsing eap-config failed</exception>
		private async Task<bool> HandleProfileSelect(string profileId, string eapConfigXml = null, bool skipOverview = false)
		{
			LoadPageLoading();
			IdentityProviderProfile profile = null;

			if (!string.IsNullOrEmpty(profileId)
				&& !string.IsNullOrEmpty(eapConfigXml))
			{
				// TODO: ^perhaps reuse logic from PersistingStore.IsReinstallable
				Debug.WriteLine(nameof(eapConfigXml) + " was set", category: nameof(HandleProfileSelect));

				eapConfig = EapConfig.FromXmlData(eapConfigXml);
				eapConfig.ProfileId = profileId;
			}
			else
			{
				Debug.WriteLine(nameof(eapConfigXml) + " was not set", category: nameof(HandleProfileSelect));

				profile = IdpDownloader.GetProfileFromId(profileId);
				try
				{
					eapConfig = await DownloadEapConfig(profile);
				}
				catch (EduroamAppUserException ex) // TODO: catch this on some higher level
				{
					MessageBox.Show(
						ex.UserFacingMessage,
						caption: "geteduroam - Exception");
					eapConfig = null;
				}
			}

			// reenable buttons after LoadPageLoading() disables them
			btnBack.IsEnabled = true;
			btnNext.IsEnabled = true;

			if (eapConfig != null)
			{
				if (!CheckIfEapConfigIsSupported(eapConfig))
					return false;

				if (HasInfo(eapConfig) && !skipOverview)
				{
					LoadPageProfileOverview();
					return true;
				}
				if (ConnectToEduroam.EnumerateCAInstallers(eapConfig)
						.Any(installer => installer.IsInstalledByUs || !installer.IsInstalled))
				{
					LoadPageCertificateOverview();
					return true;
				}

				LoadPageLogin();
				return true;
			}
			else if (!string.IsNullOrEmpty(profile?.redirect))
			{
				// TODO: add option to go to selectmethod from redirect
				LoadPageRedirect(new Uri(profile.redirect));
				return true;
			}
			else if (profile?.oauth ?? false)
			{

				LoadPageOAuthWait(profile);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Used to determine if an eapconfig has enough info
		/// for the ProfileOverview page to show
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		private static bool HasInfo(EapConfig config)
			=> !string.IsNullOrEmpty(config.InstitutionInfo.WebAddress)
			|| !string.IsNullOrEmpty(config.InstitutionInfo.EmailAddress)
			|| !string.IsNullOrEmpty(config.InstitutionInfo.Description)
			|| !string.IsNullOrEmpty(config.InstitutionInfo.Phone)
			|| !string.IsNullOrEmpty(config.InstitutionInfo.TermsOfUse);

		/// <summary>
		/// Gets EAP-config file, either directly or after browser authentication.
		/// Prepares for redirect if no EAP-config.
		/// </summary>
		/// <returns>EapConfig object.</returns>
		/// <exception cref="EduroamAppUserException">description</exception>
		public async Task<EapConfig> DownloadEapConfig(IdentityProviderProfile profile)
		{
			if (string.IsNullOrEmpty(profile?.Id))
				return null;

			// if OAuth
			if (profile.oauth || !string.IsNullOrEmpty(profile.redirect))
				return null;

			try
			{
				return await Task.Run(()
					=> IdpDownloader.DownloadEapConfig(profile.Id)
				);
			}
			catch (ApiUnreachableException e)
			{
				throw new EduroamAppUserException("HttpRequestException",
					"Couldn't connect to the server.\n\n" +
					"Make sure that you are connected to the internet, then try again.\n" +
					"Exception: " + e.Message);
			}
			catch (ApiParsingException e)
			{
				throw new EduroamAppUserException("xml parse exception",
					"The institution or profile is either not supported or malformed. " +
					"Please select a different institution or profile.\n\n" +
					"Exception: " + e.Message);
			}
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
			catch (EduroamAppUserException ex)
			{
				// NICE TO HAVE: log the error
				connectSuccess = false;
				MessageBox.Show("Could not connect. \nException: " + ex.UserFacingMessage);
			}
			return connectSuccess;
		}

		/// <summary>
		/// Checks if an EAP-config file exists in the same folder as the executable.
		/// If the installed and a EAP-config was bundled in a EXE using 7z, then this case will trigger
		/// </summary>
		/// <returns>EapConfig or null</returns>
		public static EapConfig GetBundledEapConfig()
		{
			string exeLocation = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string[] files = Directory.GetFiles(exeLocation, "*.eap-config");

			if (!files.Any()) return null;
			try
			{
				string eapPath = files.First();
				string eapString = File.ReadAllText(eapPath);
				var eapconfig = EapConfig.FromXmlData(eapString);

				return EduroamNetwork.IsEapConfigSupported(eapconfig)
					? eapconfig
					: null;
			}
			catch (XmlException) { }
			return null;
		}

		private static App App { get => (App)Application.Current; }

		/// <summary>
		/// Called by the Menu.OAuthWait page when the OAuth process is done.
		/// Gives the EapConfig it got from the oauth session as param
		/// </summary>
		/// <param name="eapConfig"></param>
		public void OAuthComplete(EapConfig eapConfig)
		{
			if (eapConfig == null || !CheckIfEapConfigIsSupported(eapConfig))
				eapConfig = null;

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
			Activate();

			// Set the window always on top, so that the user can continue the flow
			// without closing the web browser
			// - the user may not understand that they have to close the browser

			// Will be set to false again in OnActivated,
			// which will run at some point but apparently not directly on click
			this.Topmost = true;
		}

		/// <summary>
		/// Loads the logo form the curent eapconfig if it exists.
		/// Else display Eduroam logo.
		/// </summary>
		public void LoadProviderLogo()
		{
			ResetLogo();
			// gets institution logo encoded to base64
			byte[] logoBytes = eapConfig.InstitutionInfo.LogoData;
			string logoMimeType = eapConfig.InstitutionInfo.LogoMimeType;
			// adds logo to form if exists
			if (logoBytes.Length > 0)
			{
				// SVG
				if (logoMimeType == "image/svg+xml")
				{
					imgLogo.Visibility = Visibility.Hidden;
					webLogo.NavigateToString(ImageFunctions.GenerateSvgLogoHtml(logoBytes));
					webLogo.Visibility = Visibility.Visible;
				}
				else // other filetypes (jpg, png etc.)
				{
					webLogo.Visibility = Visibility.Hidden;
					try
					{
						// converts from base64 to image
						BitmapImage bitMapImage = ImageFunctions.LoadImage(logoBytes);
						imgLogo.Source = bitMapImage;
						imgLogo.Visibility = Visibility.Visible;
					}
					catch (FormatException)
					{
						imgEduroamLogo.Visibility = Visibility.Visible;
						imgLogo.Visibility = Visibility.Hidden;
					}
					catch (NotSupportedException)
					{
						imgEduroamLogo.Visibility = Visibility.Visible;
						imgLogo.Visibility = Visibility.Hidden;
					}
				}
			}
		}

		/// <summary>
		/// Hides insstitution logos and show geteduroam logo
		/// </summary>
		public void ResetLogo()
		{
			imgLogo.Visibility = Visibility.Hidden;
			webLogo.Visibility = Visibility.Hidden;
			imgEduroamLogo.Visibility = Visibility.Visible;
		}

		public void LoadPageInstalledProfile()
		{
			currentFormId = FormId.InstalledProfile;
			btnBack.IsEnabled = false;
			btnBack.Visibility = Visibility.Hidden;
			btnNext.Visibility = Visibility.Hidden;
			btnSettings.Visibility = Visibility.Visible;
			btnHelp.Visibility = Visibility.Visible;
			miLoadEapFile.IsEnabled = true;
			miRefresh.IsEnabled = PersistingStore.IsRefreshable;
			miReauthenticate.IsEnabled = PersistingStore.IsRefreshable || PersistingStore.IsReinstallable;
			miRemove.IsEnabled = PersistingStore.IsRefreshable || PersistingStore.IsReinstallable;
			miClearRootCerts.IsEnabled = !PersistingStore.IsRefreshable && !PersistingStore.IsReinstallable && CertificateStore.AnyRootCaInstalledByUs();
			miUninstall.IsEnabled = App.Installer.IsInstalled;
			pageInstalledProfile = new InstalledProfile(this);
			Navigate(pageInstalledProfile);
		}

		public void LoadPageMainMenu(bool refresh = true)
		{
			PresetUsername = null;
			ExtractFlag = false;
			currentFormId = FormId.MainMenu;
			btnNext.Visibility = Visibility.Hidden;
			btnBack.IsEnabled = PersistingStore.IdentityProvider != null;
			btnBack.Visibility = PersistingStore.IdentityProvider == null ? Visibility.Hidden : Visibility.Visible;
			btnSettings.Visibility = Visibility.Visible;
			btnHelp.Visibility = Visibility.Visible;
			miLoadEapFile.IsEnabled = true;
			miRefresh.IsEnabled = PersistingStore.IsRefreshable;
			miReauthenticate.IsEnabled = PersistingStore.IsRefreshable || PersistingStore.IsReinstallable;
			miRemove.IsEnabled = PersistingStore.IsRefreshable || PersistingStore.IsReinstallable;
			miClearRootCerts.IsEnabled = !PersistingStore.IsRefreshable && !PersistingStore.IsReinstallable && CertificateStore.AnyRootCaInstalledByUs();
			miUninstall.IsEnabled = App.Installer.IsInstalled;
			ResetLogo();
			if (refresh) pageMainMenu = new MainMenu(this);
			Navigate(pageMainMenu);
		}

		public void LoadPageSelectInstitution(bool refresh = true)
		{
			PresetUsername = null;
			currentFormId = FormId.SelectInstitution;
			btnNext.Visibility = Visibility.Visible;
			btnNext.Content = "Next";
			btnBack.IsEnabled = true;
			btnBack.Visibility = Visibility.Visible;
			btnSettings.Visibility = Visibility.Hidden;
			btnHelp.Visibility = Visibility.Hidden;
			ResetLogo();
			if (refresh) pageSelectInstitution = new SelectInstitution(this);

			Navigate(pageSelectInstitution);
		}

		public void LoadPageSelectProfile(bool refresh = true)
		{
			PresetUsername = null;
			currentFormId = FormId.SelectProfile;
			btnNext.Visibility = Visibility.Visible;
			btnNext.Content = "Next";
			btnSettings.Visibility = Visibility.Hidden;
			btnHelp.Visibility = Visibility.Hidden;
			ResetLogo();
			if (refresh) pageSelectProfile = new SelectProfile(this, pageSelectInstitution.IdProviderId);
			Navigate(pageSelectProfile);
		}

		public void LoadPageProfileOverview(bool refresh = true)
		{
			currentFormId = FormId.ProfileOverview;
			btnNext.Visibility = Visibility.Visible;
			btnNext.IsEnabled = true;
			btnNext.Content = "Next";
			btnBack.IsEnabled = true;
			btnBack.Visibility = Visibility.Visible;
			btnSettings.Visibility = Visibility.Hidden;
			btnHelp.Visibility = Visibility.Hidden;
			if (refresh) pageProfileOverview = new ProfileOverview(this, eapConfig);
			Navigate(pageProfileOverview);
		}

		public void LoadPageCertificateOverview(bool refresh = true)
		{
			// if all certificates are installed then skip to login
			currentFormId = FormId.CertificateOverview;
			btnBack.Visibility = Visibility.Visible;
			btnBack.IsEnabled = true;
			btnNext.Visibility = Visibility.Visible;
			btnNext.Content = "Next";
			btnSettings.Visibility = Visibility.Hidden;
			btnHelp.Visibility = Visibility.Hidden;
			if (refresh) pageCertificateOverview = new CertificateOverview(this, eapConfig);
			Navigate(pageCertificateOverview);
		}

		public void LoadPageLogin(bool refresh = true)
		{
			currentFormId = FormId.Login;
			btnBack.IsEnabled = true;
			btnBack.Visibility = Visibility.Visible;
			btnSettings.Visibility = Visibility.Hidden;
			btnHelp.Visibility = Visibility.Hidden;
			btnSettings.Visibility = Visibility.Hidden;
			btnHelp.Visibility = Visibility.Hidden;
			if (refresh) pageLogin = new Login(this, eapConfig);
			Navigate(pageLogin);
		}

		public void LoadPageRedirect(Uri redirect, bool refresh = true)
		{
			currentFormId = FormId.Redirect;
			btnBack.IsEnabled = true;
			btnNext.IsEnabled = false;
			btnSettings.Visibility = Visibility.Hidden;
			btnHelp.Visibility = Visibility.Hidden;
			if (refresh) pageRedirect = new Redirect(this, redirect);
			Navigate(pageRedirect);
		}

		public void LoadPageLoading(bool refresh = true)
		{
			currentFormId = FormId.Loading;
			btnBack.IsEnabled = false;
			btnNext.IsEnabled = false;
			btnSettings.Visibility = Visibility.Hidden;
			btnHelp.Visibility = Visibility.Hidden;
			if (refresh) pageLoading = new Loading(this);
			Navigate(pageLoading);
		}


		public void LoadPageTermsOfUse(bool refresh = true)
		{
			currentFormId = FormId.TermsOfUse;
			btnBack.IsEnabled = true;
			btnNext.Content = "OK";
			btnNext.Visibility = Visibility.Visible;
			btnSettings.Visibility = Visibility.Hidden;
			btnHelp.Visibility = Visibility.Hidden;
			if (refresh) pageTermsOfUse = new TermsOfUse(this, eapConfig.InstitutionInfo.TermsOfUse);
			Navigate(pageTermsOfUse);
		}

		public void LoadPageOAuthWait(IdentityProviderProfile profile)
		{
			currentFormId = FormId.OAuthWait;
			btnBack.IsEnabled = true;
			btnBack.Visibility = Visibility.Visible;
			btnNext.IsEnabled = false;
			btnSettings.Visibility = Visibility.Hidden;
			btnHelp.Visibility = Visibility.Hidden;
			pageOAuthWait = new OAuthWait(this, profile);
			Navigate(pageOAuthWait);
		}


		private bool IsShuttingDown = false;
		public void Shutdown(int exitCode = 0)
		{
			IsShuttingDown = true;
			Application.Current.Shutdown(exitCode);
		}

		private void Reauthenticate()
		{
			historyFormId.Add(currentFormId);
			PresetUsername = PersistingStore.Username;
			_ = HandleProfileSelect(
				PersistingStore.IdentityProvider?.ProfileId,
				PersistingStore.IdentityProvider?.EapConfigXml,
				skipOverview: true);
		}

		private void btnNext_Click(object sender, RoutedEventArgs e)
			=> NextPage();

		private void btnBack_Click(object sender, RoutedEventArgs e)
			=> PreviousPage();
		private async void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			var focused = FocusManager.GetFocusedElement(this);
			if (e.Key == System.Windows.Input.Key.BrowserBack
				|| e.Key == System.Windows.Input.Key.Escape
				|| (e.Key == System.Windows.Input.Key.Back && !(focused is TextBox || focused is PasswordBox)))
			{
				btnBack.Style = (Style)App.Resources["BlueButtonStyle"];
				// the button style is now blue
				// Let it stay blue for 100 milliseconds so the user sees the button flash
				// so it is clear which button was activated by pressing backspace
				await Task.Delay(100);
				PreviousPage();
			}
		}

		private void OnActivated(object sender, EventArgs e)
		{
			// Disable always on top if it was set in OAuthComplete
			this.Topmost = false;
		}

		// Logic to minimize to tray:
		private void OnWindowClose(object sender, CancelEventArgs e)
		{
			Debug.WriteLine("Event: OnClose");
			Debug.WriteLine("Sender: " + sender.ToString());
			Debug.WriteLine("{0}: {1}", nameof(IsShuttingDown), IsShuttingDown);
			Debug.WriteLine("{0}: {1}", nameof(App.Installer.IsInstalled), App.Installer.IsInstalled);
			Debug.WriteLine("{0}: {1}", nameof(App.Installer.IsRunningInInstallLocation), App.Installer.IsRunningInInstallLocation);
		}

		private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
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


		/// <summary>
		/// Disables WPF history nagivation.
		/// </summary>
		private void Main_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
		{
			if (e.NavigationMode != NavigationMode.New)
				e.Cancel = true;
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
			=> Close();

		private void MouseStartWindowDrag(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			//base.OnMouseLeftButtonDown(e);
			DragMove();
		}

		private void btnSettings_Click(object sender, RoutedEventArgs e)
		{
			Button btnSender = (Button)sender;
			Point ptLowerLeft = new Point(0, btnSender.Height);
			ctMenuSettings.IsOpen = true;
		}

		private void miLoadEapFile_Click(object sender, RoutedEventArgs e)
		{
			LocalEapConfig = FileDialog.AskUserForEapConfig();
			if (LocalEapConfig == null)
				LocalEapConfig = null;
			else if (!MainWindow.CheckIfEapConfigIsSupported(LocalEapConfig))
				LocalEapConfig = null;
			if (LocalEapConfig == null) return;

			NextPage();

			// Reset the local config, so that we don't accidentally override the button to the discovery
			LocalEapConfig = null;
		}
		private async void miRefresh_Click(object sender, RoutedEventArgs e)
		{
			var response = LetsWifi.RefreshResponse.Failed;
			try
			{
				response = await LetsWifi.RefreshAndInstallEapConfig(force: true, onlyLetsWifi: true);
			}
			catch (ApiParsingException ex)
			{
				MessageBox.Show(ex.Message, "Unable to refresh", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (HttpRequestException)
			{
				Reauthenticate();
				return;
			}
			switch (response)
			{
				case LetsWifi.RefreshResponse.Success:
				case LetsWifi.RefreshResponse.UpdatedEapXml: // Should never happen due to onlyLetsWifi=true
					pageInstalledProfile.LoadCertInfo();
					break;
				case LetsWifi.RefreshResponse.StillValid: // should never happend due to force=true
				case LetsWifi.RefreshResponse.AccessDenied:
				case LetsWifi.RefreshResponse.NewRootCaRequired:
				case LetsWifi.RefreshResponse.NotRefreshable:
				case LetsWifi.RefreshResponse.Failed:
					break;
			}
		}
		private void miReauthenticate_Click(object sender, RoutedEventArgs e)
		{
			Reauthenticate();
		}

		private void miRemove_Click(object sender, RoutedEventArgs e)
		{
			string profileName = PersistingStore.IdentityProvider?.DisplayName ?? "geteduroam";
			if (MessageBoxResult.OK == MessageBox.Show(
					"This will remove all configuration for\r\n" + profileName,
					"Remove " + profileName,
					MessageBoxButton.OKCancel,
					MessageBoxImage.Warning
					))
			{
				App.RemoveSettings(omitRootCa: true);
				LoadPageMainMenu();
			}
		}
		private void miClearRootCerts_Click(object sender, RoutedEventArgs e)
		{
			CertificateStore.UninstallAllInstalledCertificates(omitRootCa: false);
			miClearRootCerts.IsEnabled = !PersistingStore.IsRefreshable && !PersistingStore.IsReinstallable && CertificateStore.AnyRootCaInstalledByUs();
		}

		private void miUninstall_Click(object sender, RoutedEventArgs e)
			=> App.PromptAndUninstallSelf(_ => App.Shutdown());

		private void btnHelp_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("https://www.geteduroam.app/");
		}
	}
}
