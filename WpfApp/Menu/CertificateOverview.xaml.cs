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
using WpfApp.Classes;

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for InstallCertificates.xaml
	/// </summary>
	public partial class CertificateOverview : Page, IObserver<ConnectToEduroam.CertificateInstaller>
	{
		private IDisposable unsubscriber;
		private readonly MainWindow mainWindow;
		private readonly EapConfig eapConfig;
		private List<ConnectToEduroam.CertificateInstaller> installers;
		public CertificateOverview(MainWindow mainWindow, EapConfig eapConfig)
		{
			this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
			this.eapConfig = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));
			InitializeComponent();
			Load();
		}

		private void Load()
		{
			tbInfo.Text = "In order to continue you have to install the listed certificates";
			installers = ConnectToEduroam.EnumerateCAInstallers(eapConfig).ToList();
			foreach (ConnectToEduroam.CertificateInstaller installer in installers )
			{
				AddSeparator();
				AddCertGrid(installer);
			}
			// remove the first separator
			stpCerts.Children.RemoveAt(0);

			VerifyNextButton();
		}

		private void AddCertGrid( ConnectToEduroam.CertificateInstaller installer)
		{
			CertificateGrid grid = new CertificateGrid
			{
				Margin = new Thickness(5, 5, 5, 5),
				Installer = installer,
			};
			unsubscriber = grid.Subscribe(this);
			AddToStack(grid);
		}

		private void AddSeparator()
		{
			Separator sep = new Separator
			{
				Height = 1,
				BorderThickness = new Thickness(1, 1, 1, 1),
				BorderBrush = Brushes.LightGray,
				Margin = new Thickness(5, 0, 5, 0)
			};
			AddToStack(sep);
		}

		private void VerifyNextButton()
		{
			bool isInstalled = VerifyInstallers();
			mainWindow.btnNext.IsEnabled = isInstalled;
			if (isInstalled)
			{
				tbInfo.Text = "All Certificates are installed";
			}
		}

		/// <summary>
		/// checks if all certificates are installed
		/// </summary>
		/// <returns>true if all certs are installed. Else return is false</returns>
		private bool VerifyInstallers()
		{
			foreach (ConnectToEduroam.CertificateInstaller installer in installers)
			{
				if (!installer.IsInstalled) return false;
			}
			return true;
		}

		private void AddToStack(Control c)
		{
			stpCerts.Children.Add(c);
		}

		public void OnNext(ConnectToEduroam.CertificateInstaller value)
		{
			VerifyNextButton();
		}

		public void OnError(Exception error)
		{
			throw new NotImplementedException();
		}

		public void OnCompleted()
		{
			throw new NotImplementedException();
		}
	}
}
