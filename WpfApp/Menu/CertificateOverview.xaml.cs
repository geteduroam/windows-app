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
	public partial class CertificateOverview : Page
	{
		private MainWindow mainWindow;
		private EapConfig eapConfig;
		public CertificateOverview(MainWindow mainWindow, EapConfig eapConfig)
		{
			this.mainWindow = mainWindow;
			this.eapConfig = eapConfig;
			InitializeComponent();
			Load();
		}

		private void Load()
		{
			tbInfo.Text = "In order to continue you have to install the listed certificates";
			var certs = ConnectToEduroam.EnumerateCAs(eapConfig).ToList();
			foreach (ConnectToEduroam.CertificateInstaller installer in certs )
			{
				AddSeparator();
				AddCertGrid(installer);
			}
		}

		private void AddCertGrid( ConnectToEduroam.CertificateInstaller installer)
		{
			CertificateGrid grid = new CertificateGrid
			{
				Margin = new Thickness(5, 5, 5, 5),
				Installer = installer,
			};

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

		private void AddToStack(Control c)
		{
			stpCerts.Children.Add(c);
		}
	}
}
