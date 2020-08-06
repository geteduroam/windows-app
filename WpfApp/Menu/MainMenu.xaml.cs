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
	/// Interaction logic for MainMenu.xaml
	/// </summary>
	public partial class MainMenu : Page
	{
		private readonly MainWindow mainWindow;
		public EapConfig LocalEapConfig { get; set; }
		public MainMenu(MainWindow mainWindow)

		{
			this.mainWindow = mainWindow;
			InitializeComponent();
			//btnExisting.Visibility = Visibility.Collapsed;
			LoadPage();
		}

		private void LoadPage()
		{
			// mainWindow.lblTitle.Content = "Connect to Eduroam";
			mainWindow.btnNext.Visibility = Visibility.Hidden;
			mainWindow.btnBack.Visibility = Visibility.Hidden;
			//lblInfo.Visibility = Visibility.Collapsed;
			tbInfo.Visibility = Visibility.Hidden;
			//mainWindow.lblTitle.Visibility = Visibility.Hidden;
			//btnExisting.IsEnabled = true;
		}

		private void btnNewProfile_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.NextPage();
		}

		private void btnExisting_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnFile_Click(object sender, RoutedEventArgs e)
		{
			LocalEapConfig = FileDialog.AskUserForEapConfig();
			mainWindow.NextPage();
		}
	}
}
