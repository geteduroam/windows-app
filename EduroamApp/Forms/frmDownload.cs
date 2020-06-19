using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EduroamApp
{
	/// <summary>
	/// Lets the user select a country, institution and optionally a profile, and downloads an appropriate EAP-config file.
	/// </summary>
	public partial class frmDownload : Form
	{
		private readonly frmParent frmParent; // makes parent form accessible from this class
		private List<IdentityProvider> identityProviders = new List<IdentityProvider>(); // list containing all identity providers
		private List<Country> countries = new List<Country>();
		private IdentityProviderProfile idProviderProfiles; // list containing all profiles of an identity provider
		private int idProviderId; // id of selected institution
		public string profileId { get; set; } // id of selected institution profile

		public frmDownload(frmParent parentInstance)
		{
			// gets parent form instance
			frmParent = parentInstance;
			InitializeComponent();
		}

		private async void frmDownload_Load(object sender, EventArgs e)
		{
			// hides certain controls while loading
			HideControls();
			// displays loading animation while fetching list of institutions
			tlpLoading.Visible = true;
			// resets redirect url
			frmParent.RedirectUrl = "";

			// async method to get list of institutions
			bool getInstSuccess = await Task.Run(() => GetAllInstitutions());

			if (getInstSuccess)
			{
				// enables controls
				lblCountry.Visible = true;
				lblInstitution.Visible = true;
				cboCountry.Visible = true;
				cboInstitution.Visible = true;
				tlpLoading.Visible = false;
				frmParent.BtnNextEnabled = true;

				// populates countries combobox
				PopulateCountries();
			}
			else
			{
				tlpLoading.Visible = false;
				lblError.Visible = true;
			}
		}

		/// <summary>
		/// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
		/// </summary>
		private bool GetAllInstitutions()
		{
			try
			{
				identityProviders = IdentityProviderDownloader.GetAllIdProviders();
				return true;
			}
			catch (EduroamAppUserError ex)
			{
				lblError.Text = ex.UserFacingMessage;
			}
			return false;
		}

		/// <summary>
		/// Converts country codes from identity provider json to country names, and loads them into combo box.
		/// Next, gets user's location and chooses closest country by default.
		/// </summary>
		private void PopulateCountries()
		{
			// get all countries
			countries = IdentityProviderParser.GetCountries(identityProviders);

			// adds countries to combobox
			cboCountry.Items.AddRange(countries.OrderBy(c => c.CountryName).Select(c => c.CountryName).ToArray());

			//Find the country code for the country closest to this machine
			string closestCountryCode = IdentityProviderParser.GetClosestCountryCode(identityProviders, frmParent.GeoWatcher.Position.Location);

			// search countries for match on closestCountryCode
			string closestCountry = countries.Where(c => c.CountryCode == closestCountryCode).Select(c => c.CountryName).FirstOrDefault();

			// select closest country to be default select
			cboCountry.SelectedIndex = cboCountry.FindStringExact(closestCountry);
		}

		// populates identity provider combo box
		private void cboCountry_SelectedIndexChanged(object sender, EventArgs e)
		{
			// clear combobox
			cboInstitution.Items.Clear();
			cboProfiles.Items.Clear();
			// hide profile combobox
			lblSelectProfile.Visible = false;
			cboProfiles.Visible = false;
			// clear selected profile
			profileId = null;

			// gets country code of selected country
			string selectedCountryCode = countries.Where(c => c.CountryName == cboCountry.Text).Select(c => c.CountryCode).FirstOrDefault();

			// get all institues in selected country
			List<IdentityProvider> providersInCountry = identityProviders.Where(provider => provider.Country == selectedCountryCode)
											.OrderBy(provider => provider.Title).ToList();
			// adds identity providers from selected country to combobox
			cboInstitution.Items.AddRange(providersInCountry.Select(provider => provider.Title).ToArray());

			try
			{
				// find closest insitute in the country
				IdentityProvider closestInstitute = IdentityProviderParser.GetClosestIdProvider(providersInCountry, frmParent.GeoWatcher.Position.Location);
				// select closest institute to be default select
				cboInstitution.SelectedIndex = cboInstitution.FindStringExact(closestInstitute.Title);
			} catch (EduroamAppUserError ex)
			{

			}

		}

		// gets identity provider profiles, and populates profile combo box if more than one
		private void cboInstitution_SelectedIndexChanged(object sender, EventArgs e)
		{
			// clear combobox
			cboProfiles.Items.Clear();
			// clear selected profile
			profileId = null;

			// gets id of institution selected in combobox
			idProviderId = identityProviders.Where(x => x.Title == cboInstitution.Text).Select(x => x.Id).First();

			// get IdentityProviderProfile object for provider, containing all profiles
			try
			{
				idProviderProfiles = IdentityProviderDownloader.GetIdentityProviderProfiles(idProviderId);
			}
			catch (EduroamAppUserError ex)
			{
				EduroamAppExceptionHandler(ex);
				return;
			}

			// if an identity provider has more than one profile, add to combobox
			if (idProviderProfiles.Data.Count > 1)
			{
				// show combobox
				cboProfiles.Visible = true;
				// show label
				lblSelectProfile.Visible = true;
				// add profiles to combobox
				cboProfiles.Items.AddRange(idProviderProfiles.Data.Select(profile => profile.Display).ToArray());
			}
			else
			{
				// gets the only profile id
				profileId = idProviderProfiles.Data.Single().Id;
				// hide combobox
				cboProfiles.Visible = false;
				// hide label
				lblSelectProfile.Visible = false;
			}
		}

		// gets profile id of selected profile
		private void cboProfiles_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cboProfiles.Text != "")
			{
				// gets profile id of profile selected in combobox
				profileId = idProviderProfiles.Data.Where(profile => profile.Display == cboProfiles.Text).Select(x => x.Id).Single();
			}
		}


		/// <summary>
		/// Handles EduroamApp exxceptions.
		/// </summary>
		/// <param name="ex">WebException.</param>
		private void EduroamAppExceptionHandler(EduroamAppUserError ex)
		{
			//HideControls();
			//lblError.Text = ex.UserFacingMessage;
			//lblError.Visible = true;
			MessageBox.Show(ex.UserFacingMessage, "eduroam - Web exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}



		/// <summary>
		/// Handles exceptions related to deserializing JSON files and corrupted XML files.
		/// </summary>
		/// <param name="ex">Exception.</param>
		private void EapExceptionHandler(Exception ex)
		{
			MessageBox.Show("The selected institution or profile is not supported. " +
							"Please select a different institution or profile.\n"
							+ "Exception: " + ex.Message);
		}

		/// <summary>
		/// Hides all controls on form.
		/// </summary>
		private void HideControls()
		{
			lblError.Visible = false;
			lblCountry.Visible = false;
			lblInstitution.Visible = false;
			lblSelectProfile.Visible = false;
			cboCountry.Visible = false;
			cboInstitution.Visible = false;
			cboProfiles.Visible = false;
			frmParent.BtnNextEnabled = false;
		}
	}
}
