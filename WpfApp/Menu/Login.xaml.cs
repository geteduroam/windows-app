using EduroamConfigure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for Login.xaml
	/// </summary>
	public partial class Login : Page
	{
		private readonly MainWindow mainWindow;
		private readonly EapConfig eapConfig;

		public Login(MainWindow mainWindow, EapConfig eapConfig)
		{
			this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
			this.eapConfig = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));

			if (eapConfig.NeedsLoginCredentials())
			{
				// TODO: show input fields
			}

			if (eapConfig.NeedsClientCertificatePassphrase())
			{
				// TODO: show input field
				// This field should write to this:
				var success = eapConfig.AddClientCertificatePassphrase("asd");
			}

			InitializeComponent();
		}


		/// <summary>
		/// Installs certificates from EapConfig and creates wireless profile.
		/// </summary>
		/// <returns>true on success</returns>
		private bool InstallEapConfig(EapConfig eapConfig, string username = null, string password = null)
		{
			if (!EduroamNetwork.EapConfigIsSupported(eapConfig))
			{
				MessageBox.Show(
					"The profile you have selected is not supported by this application.",
					"No supported authentification method ws found.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return false;
			}

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

					success = true;
					break;
				}

				// TODO: move this out
				if (!EduroamNetwork.IsEduroamAvailable(eapConfig))
				{
					//err = "eduroam not available";
				}

				// TODO: remove
				mainWindow.ProfileCondition = MainWindow.ProfileStatus.Incomplete;

				return success;
			}
			catch (CryptographicException cryptEx) // TODO, handle in ConnectToEduroam or EduroamNetwork, thrown by X509Certificate2 constructor or store.add()
			{
				MessageBox.Show(
					"One or more certificates are corrupt. Please select an another file, or try again later.\n" +
					"\n" +
					"Exception: " + cryptEx.Message,
					"eduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			catch (EduroamAppUserError ex)
			{
				MessageBox.Show(
					ex.UserFacingMessage,
					"eduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			catch (Exception ex) // TODO, handle in ConnectToEduroam or EduroamNetwork
			{
				MessageBox.Show(
					"Something went wrong.\n" +
					"Please try connecting with another institution, or try again later.\n" +
					"\n" +
					"Exception: " + ex.Message,
					"eduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			return false;
		}

	}
}
