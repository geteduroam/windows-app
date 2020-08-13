using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EduroamConfigure;

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for SelectInstitution.xaml
	/// </summary>
	public partial class SelectInstitution : Page
	{
		private readonly MainWindow mainWindow;
		private IdentityProviderDownloader downloader;
		private bool isSearching;
		private bool isNewSearch;
		public int IdProviderId { get; private set; } // id of selected institution

		public SelectInstitution(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
			InitializeComponent();
			LoadPage();
		}

		// TODO: add loading image
		private async void LoadPage()
		{

			mainWindow.btnNext.IsEnabled = false;
			downloader = mainWindow.IdpDownloader;

			await Task.Run(() => PopulateInstitutions());

			tbTitle.Text = "Select institution";
		}


		/// <summary>
		/// Called when the form is created to present the 10 closest providers
		/// </summary>
		private void PopulateInstitutions()
		{
			UpdateInstitutions(downloader.ClosestProviders);
		}

		/// <summary>
		/// Used to update institution list portrayed to users.
		/// Called by different thread than Winform thread
		/// </summary>
		private void UpdateInstitutions(List<IdentityProvider> institutions)
		{
			this.Dispatcher.Invoke(() => {
				lbInstitutions.ItemsSource = institutions;
			});
		}

		private void lbInstitution_DoubleClick(object sender, EventArgs e)
		{
			mainWindow.NextPage();
		}

		private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			Search();
		}

		/// <summary>
		/// Search function called when the search box is changed.
		/// Does a search through all institutions and sorts it
		/// to best match the search terms
		/// </summary>
		private async void Search()
		{
			// flag so only one search is done at a time
			if (isSearching)
			{
				// remember that a new search term got written so its not forgotten
				isNewSearch = true;
				return;
			}
			// isSearching was false, so current searchString is newest
			isNewSearch = false;
			string searchString = tbSearch.Text;
			isSearching = true;
			// async to hinder UI from freezing
			await Task.Run(() => UpdateInstitutions(
					IdentityProviderParser.SortByQuery(
						downloader.ClosestProviders,
						searchString,
						limit: 100)));
			isSearching = false;
			// if search text has changed during await then run the newest search string so its not lost
			if (isNewSearch) Search();
		}

		private void lbInstitutions_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// if user clicks on empty area of the listbox it will cause event but no item is selected
			// this is legacy from winforms, unknown if this is true for wpf
			if (lbInstitutions.SelectedItem == null) return;

			// select provider ID from selected provider
			IdentityProvider selectedProvider = (IdentityProvider)lbInstitutions.SelectedItem;
			IdProviderId = selectedProvider.cat_idp;

			mainWindow.btnNext.IsEnabled = true;
		}

		private void lbInstitutions_MouseDoubleClick(object sender, RoutedEventArgs e)
		{
			if (lbInstitutions.SelectedItem != null)
			{
				mainWindow.NextPage();
			}
		}
	}

}
