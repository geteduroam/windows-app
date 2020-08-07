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
		public bool UseExtracted { get; set; }
		public MainMenu(MainWindow mainWindow)

		{
			this.mainWindow = mainWindow;
			InitializeComponent();
			Load();
		}

		private void Load()
		{
			mainWindow.btnNext.Visibility = Visibility.Hidden;
			mainWindow.btnBack.Visibility = Visibility.Hidden;
			tbInfo.Visibility = Visibility.Hidden;

			if(mainWindow.ExtractedEapConfig == null)
			{
				btnExisting.Visibility = Visibility.Collapsed;
			}
			else
			{
				btnExisting.Visibility = Visibility.Visible;
				tbExisting.Text = "Connect with " + mainWindow.ExtractedEapConfig.InstitutionInfo.DisplayName;
			}
			if(!mainWindow.Online)
			{
				BtnNewProfile.Content = "No internet connection";
				BtnNewProfile.IsEnabled = false;
			}

		}

		private void btnNewProfile_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.NextPage();
		}

		private void btnExisting_Click(object sender, RoutedEventArgs e)
		{
			UseExtracted = true;
			mainWindow.ExtractFlag = true;
			mainWindow.NextPage();
		}

		private void btnFile_Click(object sender, RoutedEventArgs e)
		{
			LocalEapConfig = FileDialog.AskUserForEapConfig();
			if (LocalEapConfig == null) return;
			mainWindow.NextPage();
		}
	}
}
