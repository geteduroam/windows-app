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
	/// Interaction logic for SelectProfile.xaml
	/// </summary>
	public partial class SelectProfile : Page
	{
		private readonly MainWindow mainWindow; // makes parent form accessible from this class
		private readonly int IdProviderId; // id of selected institution
		public string ProfileId { get; set; } // id of selected institution profile

		public SelectProfile(MainWindow mainWindow, int providerId)
		{
			this.mainWindow = mainWindow;
			this.IdProviderId = providerId;
			InitializeComponent();
			LoadPage();
		}

		private async void LoadPage()
		{
		  //  frmParent.WebEduroamLogo.Visible = true;
			mainWindow.btnNext.IsEnabled = false;



			//  frmParent.RedirectUrl = "";
			lbProfiles.IsEnabled = false;

			await Task.Run(() => PopulateProfiles());

			lbProfiles.IsEnabled = true;

			// autoselect first profile
			// LbProfiles.SetSelected(0, true);

		}

		/// <summary>
		/// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
		/// </summary>
		private void PopulateProfiles()
		{
			var idProviderProfiles = mainWindow.IdpDownloader.GetIdentityProviderProfiles(IdProviderId);
			this.Dispatcher.Invoke(() => {
				lbProfiles.ItemsSource = idProviderProfiles;
			});

		}

		/// <summary>
		/// Called when user selects a profile
		/// </summary>
		private void LbProfiles_SelectionChanged(object sender, EventArgs e)
		{
			// if user clicks on empty area of the listbox it will cause event but no item is selected
			if (lbProfiles.SelectedItem == null) return;

			// gets id of selected profile
			var selectedProfile = (IdentityProviderProfile) lbProfiles.SelectedItem;
			ProfileId = selectedProfile.Id;
			mainWindow.btnNext.IsEnabled = true;
		}


	}
}
