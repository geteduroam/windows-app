using EduroamConfigure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp.Classes;
using System.Globalization;

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for Login.xaml
	/// </summary>
	public partial class Login : Page
	{

		private enum ConType
		{
			Credentials,
			CertPass,
			CertAndCertPass,
			Nothing,
		}
		private ConType conType;
		private bool usernameValid = false;
		private string realm;
		private bool hint;
		private Control focused;
		private string filepath;
		private DateTime certValid;
		public DispatcherTimer dispatcherTimer { get; set; }
		public bool IsConnected { get; set; }
		public bool IgnorePasswordChange { get; set; }



		private readonly MainWindow mainWindow;
		private readonly EapConfig eapConfig;



		public Login(MainWindow mainWindow, EapConfig eapConfig)
		{
			this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
			this.eapConfig = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));
			InitializeComponent();
			Load();
		}

		private void Load()
		{
			gridCred.Visibility = Visibility.Collapsed;
			gridCertPassword.Visibility = Visibility.Collapsed;
			gridCertBrowser.Visibility = Visibility.Collapsed;
			stpTime.Visibility = Visibility.Collapsed;
			eapConfig.AuthenticationMethods.First();
			mainWindow.btnNext.IsEnabled = false;
			mainWindow.btnNext.Content = "Connect";

			dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
			dispatcherTimer.Interval = new TimeSpan(0, 0, 1);

			if (eapConfig.NeedsLoginCredentials())
			{
				// TODO: show input fields
				conType = ConType.Credentials;
				grpRules.Visibility = Visibility.Hidden;
				gridCred.Visibility = Visibility.Visible;
				tbRules.Visibility = Visibility.Visible;
				(realm, hint) = eapConfig.GetClientInnerIdentityRestrictions();
				tbRealm.Text = '@' + realm;
				tbRealm.Visibility = !string.IsNullOrEmpty(realm) && hint ? Visibility.Visible : Visibility.Hidden;
				tbUsername.Focus();
				ValidateCredFields();
			}
			else if (eapConfig.NeedsClientCertificate())
			{
				gridCertBrowser.Visibility = Visibility.Visible;
				conType = ConType.CertAndCertPass;
			}
			else if (eapConfig.NeedsClientCertificatePassphrase())
			{
				conType = ConType.CertPass;
				gridCertPassword.Visibility = Visibility.Visible;
				mainWindow.btnNext.IsEnabled = true;
				//var success = eapConfig.AddClientCertificatePassphrase("asd");
			}
			else
			{
				// just connnect
				conType = ConType.Nothing;
				ConnectClick();
			}
		}

		public bool ValidateCredFields()
		{
			string username = tbUsername.Text;
			if (string.IsNullOrEmpty(username))
			{
				usernameValid = false;
				tbRules.Text = "";
				mainWindow.btnNext.IsEnabled = false;
				return false;
			}

			// if username does not contain '@' and realm is given then show realm added to end
			if ((!username.Contains('@') && !string.IsNullOrEmpty(realm)) || hint)
			{
				username += "@" + realm;
			}

			var brokenRules = IdentityProviderParser.GetRulesBroken(username, realm, hint).ToList();
			usernameValid = !brokenRules.Any();
			tbRules.Text = "";
			if (!usernameValid)
			{
				tbRules.Text = string.Join("\n", brokenRules); ;
			}

			bool fieldsValid = (!string.IsNullOrEmpty(pbCredPassword.Password) && usernameValid) || IsConnected;
			bool isvalid = credentialsValid();
			mainWindow.btnNext.IsEnabled = isvalid;
			return isvalid;
		}

		private bool credentialsValid()
		{

			string username = tbUsername.Text;
			if (string.IsNullOrEmpty(username))
			{
				usernameValid = false;
				tbRules.Text = "";
				mainWindow.btnNext.IsEnabled = false;
				return false;
			}

			// if username does not contain '@' and realm is given then show realm added to end
			if ((!username.Contains('@') && !string.IsNullOrEmpty(realm)) || hint)
			{
				username += "@" + realm;
			}

			var brokenRules = IdentityProviderParser.GetRulesBroken(username, realm, hint).ToList();
			usernameValid = !brokenRules.Any();
			tbRules.Text = "";
			if (!usernameValid)
			{
				tbRules.Text = string.Join("\n", brokenRules); ;
			}

			bool fieldsValid = (!string.IsNullOrEmpty(pbCredPassword.Password) && usernameValid) || IsConnected;
			return fieldsValid;
		}
		/// <summary>
		/// Decides if user is allowed to attempt to connect.
		/// Based on if filepath and password is set
		/// </summary>
		/// <returns>True if filepath and password is set</returns>
		public void ValidateCertBrowserFields()
		{
			// mainWindow.btnNext.IsEnabled = !string.IsNullOrEmpty(filepath) && !string.IsNullOrEmpty(pbCertBrowserPassword.Password);
			mainWindow.btnNext.IsEnabled = !string.IsNullOrEmpty(filepath);
		}

		public void ConnectClick()
		{
			mainWindow.btnNext.IsEnabled = false;
			tbStatus.Text = "Connecting...";
			tbStatus.Visibility = Visibility.Visible;

			switch (conType)
			{
				case ConType.Credentials:
					ConnectWithLogin();
					break;
				case ConType.Nothing:
					ConnectWithNothing();
					break;
				case ConType.CertAndCertPass:
					ConnectWithCertAndCertPass();
					break;
				case ConType.CertPass:
					ConnectWithCertPass();
					break;
			}
		}

		public async void ConnectWithCertAndCertPass()
		{
			var success = eapConfig.AddClientCertificate(filepath, pbCertBrowserPassword.Password);

			if (success)
			{
				_ = await ConnectAndUpdateUI();
			}
			else
			{
				tbStatus.Text = "Incorrect password";
			}
			if (!dispatcherTimer.IsEnabled)
			{
				mainWindow.btnNext.IsEnabled = true;
			}
		}

		public async void ConnectWithCertPass()
		{
			var success = eapConfig.AddClientCertificatePassphrase(pbCertPassword.Password);

			if (success)
			{
				_ = await ConnectAndUpdateUI();
			}
			else
			{
				tbStatus.Text = "Incorrect password";
			}
			if (!dispatcherTimer.IsEnabled)
			{
				mainWindow.btnNext.IsEnabled = true;
			}
		}

		public async void ConnectWithLogin()
		{
			if (credentialsValid())
			{
				string username = tbUsername.Text;
				if ((!username.Contains('@') && !string.IsNullOrEmpty(realm)) || hint)
				{
					tbRealm.Visibility = Visibility.Visible;
				}

				if (tbRealm.Visibility == Visibility.Visible)
				{
					username += tbRealm.Text;
				}
				string password = pbCredPassword.Password;

				pbCredPassword.IsEnabled = false;
				tbUsername.IsEnabled = false;

				_ = await ConnectAndUpdateUI(username, password);

				pbCredPassword.IsEnabled = true;
				tbUsername.IsEnabled = true;
				if (!dispatcherTimer.IsEnabled)
				{
					mainWindow.btnNext.IsEnabled = true;
				}
				if (focused != null) focused.Focus();
			}
			else
			{
				grpRules.Visibility = string.IsNullOrEmpty(tbRules.Text) ? Visibility.Hidden : Visibility.Visible;
			}
		}

		public void ConnectWithNothing()
		{
			_ = ConnectAndUpdateUI();
		}

		public async Task<bool> ConnectAndUpdateUI(string username = null, string password = null)
		{
			bool installed = await Task.Run(() => InstallEapConfig(eapConfig, username, password));
			if (installed)
			{
				bool connected = await TryToConnect();
				if (connected)
				{
					tbStatus.Text = "You are now connected to eduroam.\n\nPress Close to exit the wizard.";
					mainWindow.btnNext.Content = "Close";
				}
				else
				{
					if (EduroamNetwork.IsEduroamAvailable(eapConfig))
					{
						tbStatus.Text = "Everything is configured!\nUnable to connect to Eduroam.";
					}
					else
					{
						// Hs2 is not enumerable
						tbStatus.Text = "Everything is configured!\nUnable to connect to Eduroam, you're probably out of coverage.";
					}
					mainWindow.btnNext.Content = "Connect";
				}
			}
			else
			{
				tbStatus.Text = "Could not install EAP-configuration";
				mainWindow.btnNext.Content = "Connect";
			}
			if (!dispatcherTimer.IsEnabled)
			{
				mainWindow.btnNext.IsEnabled = true;
			}

			return true; // to make it await-able
		}


		public async Task<bool> TryToConnect()
		{
			bool connectSuccess;
			try
			{
				connectSuccess = await Task.Run(ConnectToEduroam.TryToConnect);
			}
			catch (EduroamAppUserError ex)
			{
				connectSuccess = false;
				MessageBox.Show("Could not connect. \nException: " + ex.UserFacingMessage);
			}
			IsConnected = connectSuccess;
			return connectSuccess;
		}


		/// <summary>
		/// Installs certificates from EapConfig and creates wireless profile.
		/// </summary>
		/// <returns>true on success</returns>
		private bool InstallEapConfig(EapConfig eapConfig, string username = null, string password = null) // TODO: make static
		{
			if (!EduroamNetwork.EapConfigIsSupported(eapConfig))
			{
				MessageBox.Show(
					"The profile you have selected is not supported by this application.",
					"No supported authentification method was found.",
					MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return false;
			}

			// test
			ConnectToEduroam.RemoveAllProfiles();
			mainWindow.ProfileCondition = MainWindow.ProfileStatus.NoneConfigured;

			bool success = false;

			try
			{
				// Install EAP config as a profile
				foreach (var authMethodInstaller in ConnectToEduroam.InstallEapConfig(eapConfig))
				{
					// install intermediate CAs and client certificates
					// if user refuses to install a root CA (should never be prompted to at this stage), abort
					if (!authMethodInstaller.InstallCertificates())
						break;

					// Everything is now in order, install the profile!
					if (!authMethodInstaller.InstallProfile(username, password))
						continue; // failed, try the next method

					// check if we need to wait for the certificate to become valid
					certValid = authMethodInstaller.GetTimeWhenValid().From;
					if (DateTime.Now <= certValid)
					{
						// dispatch the event which creates the clock the end user sees
						dispatcherTimer_Tick(dispatcherTimer, new EventArgs());
						dispatcherTimer.Start();
						return false;
					}

					success = true;
					break;
				}

				// TODO: move out of function, use return value. This function should be static
				mainWindow.ProfileCondition = MainWindow.ProfileStatus.Configured;

				App.Installer.EnsureIsInstalled(); // TODO: run in backround?

				return success;
			}
			catch (EduroamAppUserError ex)
			{
				// TODO: expand the response with "try something else"
				MessageBox.Show(
					ex.UserFacingMessage,
					"geteduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					"Something went wrong.\n" +
					"Please try connecting with another profile or institution, or try again later.\n" +
					"\n" +
					"Exception: " + ex.Message,
					"geteduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			return false;
		}


		private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			this.Dispatcher.Invoke(() => {
				tbLocalTime.Text = DateTime.Now.ToString(CultureInfo.InvariantCulture);
				tbValidTime.Text = certValid.ToString(CultureInfo.InvariantCulture);
			});

			if (DateTime.Now > certValid)
			{
				dispatcherTimer.Stop();
				this.Dispatcher.Invoke(() => {
					stpTime.Visibility = Visibility.Collapsed;
					ConnectClick();
				});
			}
			else
			{
				this.Dispatcher.Invoke(() => {
					mainWindow.btnNext.IsEnabled = false;
					stpTime.Visibility = Visibility.Visible;
					tbStatus.Visibility = Visibility.Collapsed;
				});
			}
		}

		private void tbUsername_TextChanged(object sender, TextChangedEventArgs e)
		{
			tbStatus.Visibility = Visibility.Collapsed;
			grpRules.Visibility = Visibility.Collapsed;
			if (!hint) tbRealm.Visibility = Visibility.Hidden;
			ValidateCredFields();
		}

		private void tbUsername_LostFocus(object sender, RoutedEventArgs e)
		{
			grpRules.Visibility = string.IsNullOrEmpty(tbRules.Text) ? Visibility.Hidden : Visibility.Visible;
			if (!tbUsername.Text.Contains('@') && !string.IsNullOrEmpty(realm) && !string.IsNullOrEmpty(tbUsername.Text))
			{
				tbRealm.Visibility = Visibility.Visible;
			}
		}

		private void tbUsername_GotFocus(object sender, RoutedEventArgs e)
		{
			focused = tbUsername;
		}

		private void pbCredPassword_PasswordChanged(object sender, RoutedEventArgs e)
		{
			// show placeholder if no password, hide placeholder if password set.
			// in XAML a textblock is bound to tbCredPassword so when the textbox is blank a placeholder is shown
			if (IgnorePasswordChange) return;
			tbCredPassword.Text = string.IsNullOrEmpty(pbCredPassword.Password) ? "" : "something";
			tbStatus.Visibility = Visibility.Hidden;
			ValidateCredFields();
		}

		private void pbCredPassword_GotFocus(object sender, RoutedEventArgs e)
		{
			focused = pbCredPassword;
		}

		private void pbCertPassword_PasswordChanged(object sender, RoutedEventArgs e)
		{
			// show placeholder if no password, hide placeholder if password set.
			// in XAML a textblock is bound to tbCredPassword so when the textbox is blank a placeholder is shown
			if (IgnorePasswordChange) return;
			tbCertPassword.Text = string.IsNullOrEmpty(pbCertPassword.Password) ? "" : "something";
			tbStatus.Visibility = Visibility.Hidden;
		}

		private void pbCertPassword_GotFocus(object sender, RoutedEventArgs e)
		{
			focused = pbCertPassword;
		}

		private void pbCertBrowserPassword_PasswordChanged(object sender, RoutedEventArgs e)
		{
			// show placeholder if no password, hide placeholder if password set.
			// in XAML a textblock is bound to tbCredPassword so when the textbox is blank a placeholder is shown
			if (IgnorePasswordChange) return;
			tbCertBrowserPassword.Text = string.IsNullOrEmpty(pbCertBrowserPassword.Password) ? "" : "something";
			tbStatus.Visibility = Visibility.Hidden;
			ValidateCertBrowserFields();
		}

		private void pbCertBrowserPassword_GotFocus(object sender, RoutedEventArgs e)
		{
			focused = pbCertBrowserPassword;

		}

		private void btnFile_Click(object sender, RoutedEventArgs e)
		{
			//browse for certificate and add to eapconfig
			//eapConfig.AddClientCertificate();
			filepath = FileDialog.AskUserForClientCertificateBundle();
			tbCertBrowser.Text = filepath;
			ValidateCertBrowserFields();
		}

		private void btnCancelWait_Click(object sender, RoutedEventArgs e)
		{
			dispatcherTimer.Stop();
			stpTime.Visibility = Visibility.Collapsed;
			tbStatus.Visibility = Visibility.Visible;
			tbStatus.Text = "Connecting process was cancelled";
			mainWindow.btnNext.IsEnabled = true;
		}
	}
}
