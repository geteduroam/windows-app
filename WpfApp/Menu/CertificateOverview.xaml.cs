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
	/// Interaction logic for InstallCertificates.xaml
	/// </summary>
	public partial class CertificateOverview : Page
	{
		private MainWindow mainWindow;
		private EapConfig.AuthenticationMethod authMethod;
		public CertificateOverview(MainWindow mainWindow, EapConfig.AuthenticationMethod authMethod)
		{
			this.mainWindow = mainWindow;
			this.authMethod = authMethod;
			InitializeComponent();
		}
	}
}
