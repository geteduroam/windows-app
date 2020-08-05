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
		private bool InstallEapConfig(EapConfig eapConfig, string username = null, string password = null) // TODO: make static
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

				// TODO: move this out of function
				if (!EduroamNetwork.IsEduroamAvailable(eapConfig))
				{
					//err = "eduroam not available";
				}

				// TODO: move out of function, use return value. This function should be static
				mainWindow.ProfileCondition = MainWindow.ProfileStatus.Configured;

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

	}
}
