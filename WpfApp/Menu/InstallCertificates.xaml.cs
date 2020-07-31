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

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for InstallCertificates.xaml
	/// </summary>
	public partial class InstallCertificates : Page
	{
		private MainWindow mainWindow;
		public InstallCertificates(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;
			InitializeComponent();
		}
	}
}
