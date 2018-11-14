using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using System.Device.Location;
using System.Globalization;
using System.Xml;

namespace EduroamApp
{
	/// <summary>
	/// Lets the user select a country, institution and optionally a profile, and downloads an appropriate EAP-config file.
	/// </summary>
	public partial class frmDownload : Form
	{
		private readonly frmParent frmParent; // makes parent form accessible from this class
		private List<IdentityProvider> identityProviders = new List<IdentityProvider>(); // list containing all identity providers
		private readonly List<Country> countries = new List<Country>();
		private IdentityProviderProfile idProviderProfiles; // list containing all profiles of an identity provider
		private int idProviderId; // id of selected institution
		private string profileId; // id of selected institution profile

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
		public bool GetAllInstitutions()
		{
			// url for json containing all identity providers/institutions
			const string allIdentityProvidersUrl = "https://cat.eduroam.org/user/API.php?action=listAllIdentityProviders&lang=en";

			try
			{
				// downloads json file as string
				string idProviderJson = GetStringFromUrl(allIdentityProvidersUrl);
				// gets list of identity providers from json file
				identityProviders = JsonConvert.DeserializeObject<List<IdentityProvider>>(idProviderJson);
				// checks if json actually contains any identity providers, throws exception if not
				if (identityProviders.Count <= 0)
				{
					throw new JsonReaderException("Institutions couldn't be read from JSON file.");
				}
				return true;
			}
			catch (WebException)
			{
				lblError.Text = "Couldn't connect to the server.\n\n"
								+ "Make sure that you are connected to the internet, then try again.";
			}
			catch (JsonReaderException)
			{
				lblError.Text = "Selecting an institution is not possible at the moment.\n\n" +
								"Please try again later.";
			}
			return false;
		}

		/// <summary>
		/// Converts country codes from identity provider json to country names, and loads them into combo box.
		/// Next, gets user's location and chooses closest country by default.
		/// </summary>
		private void PopulateCountries()
		{
			// gets all unique country codes
			List<string> distinctCountryCodes = identityProviders.Select(provider => provider.Country)
																 .Distinct().ToList();
			// converts all country codes to country names and puts them in a list
			foreach (string countryCode in distinctCountryCodes)
			{
				string countryName;
				try
				{
					var countryInfo = new RegionInfo(countryCode);
					countryName = countryInfo.DisplayName;
				}
				// if "country" from json file does not have associated RegionInfo, set country code as country name
				catch (ArgumentException)
				{
					countryName = countryCode;
				}

				countries.Add(new Country(countryCode, countryName));
			}

			// adds countries to combobox
			cboCountry.Items.AddRange(countries.OrderBy(c => c.CountryName).Select(c => c.CountryName).ToArray());

			// gets GeoCoordinateWatcher from parent form
			GeoCoordinate myCoord = frmParent.GeoWatcher.Position.Location;
			// country closest to user
			string closestCountry;

			// checks if coordinates are received
			if (!myCoord.IsUnknown)
			{
				// finds the country geographically closest to the user and selects it by default
				string closestCountryCode = GetClosestInstitution(identityProviders, myCoord);
				// gets country from country code
				closestCountry = countries.Where(c => c.CountryCode == closestCountryCode).Select(c => c.CountryName).FirstOrDefault();
			}
			// if no coordinates
			else
			{
				// gets country as set in Settings
				// https://stackoverflow.com/questions/8879259/get-current-location-as-specified-in-region-and-language-in-c-sharp
				var regKeyGeoId = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\International\Geo");
				var geoID = (string)regKeyGeoId.GetValue("Nation");
				var allRegions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.ToString()));
				var regionInfo = allRegions.FirstOrDefault(r => r.GeoId == Int32.Parse(geoID));

				closestCountry = regionInfo.EnglishName;
			}
			// sets country as selected item in combobox
			cboCountry.SelectedIndex = cboCountry.FindStringExact(closestCountry);
		}

		/// <summary>
		/// Compares institution coordinates with user's coordinates and gets the closest institution.
		/// </summary>
		/// <param name="instList">List of all institutions.</param>
		/// <param name="userCoord">User's coordinates.</param>
		/// <returns>Country of closest institution.</returns>
		public string GetClosestInstitution(List<IdentityProvider> instList, GeoCoordinate userCoord)
		{
			// institution's coordinates
			var instCoord = new GeoCoordinate();
			// closest institution
			var closestInst = new IdentityProvider();
			// shortest distance
			double shortestDistance = double.MaxValue;

			// loops through all institutions' coordinates and compares them with current shortest distance
			foreach (IdentityProvider inst in instList)
			{
				if (inst.Geo != null) // excludes if geo property not set
				{
					// gets lat and long
					instCoord.Latitude = inst.Geo.First().Lat;
					instCoord.Longitude = inst.Geo.First().Lon;
					// gets current distance
					double currentDistance = userCoord.GetDistanceTo(instCoord);
					// compares with shortest distance
					if (currentDistance < shortestDistance)
					{
						// sets the current distance as the shortest dstance
						shortestDistance = currentDistance;
						closestInst = inst;
					}
				}
			}
			// returns country of institution closest to user
			return closestInst.Country;
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

			// adds identity providers from selected country to combobox
			cboInstitution.Items.AddRange(identityProviders.Where(provider => provider.Country == selectedCountryCode)
											.OrderBy(provider => provider.Title).Select(provider => provider.Title).ToArray());
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
			// adds institution id to url
			string profilesUrl = $"https://cat.eduroam.org/user/API.php?action=listProfiles&id={idProviderId}&lang=en";

			try
			{
				// downloads json file as string
				string profilesJson = GetStringFromUrl(profilesUrl);
				// gets identity provider profile from json
				idProviderProfiles = JsonConvert.DeserializeObject<IdentityProviderProfile>(profilesJson);
			}
			catch (WebException ex)
			{
				WebExceptionHandler(ex);
				return;
			}
			/*catch (JsonReaderException ex)
			{
				EapExceptionHandler(ex);
				return;
			}*/

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
		/// Gets a json file as string from url.
		/// </summary>
		/// <param name="url">Url containing json file.</param>
		/// <returns>Json string.</returns>
		public string GetStringFromUrl(string url)
		{
			// downloads json file from url as string
			using (var client = new WebClient())
			{
				client.Encoding = Encoding.UTF8;
				string jsonString = client.DownloadString(url);
				return jsonString;
			}
		}

		/// <summary>
		/// Gets a profile's attributes from json.
		/// </summary>
		/// <returns>Redirect link, if exists.</returns>
		public string GetProfileAttributes()
		{
			// adds profile id to url
			string profileAttributeUrl = $"https://cat.eduroam.org/user/API.php?action=profileAttributes&id={profileId}&lang=en";

			// json file as string
			//deserialized json as Profile attributes objects
			IdProviderProfileAttributes profileAttributes;

			try
			{
				// downloads json from url
				string profileAttributeJson = GetStringFromUrl(profileAttributeUrl);
				// gets profile attributes from json
				profileAttributes = JsonConvert.DeserializeObject<IdProviderProfileAttributes>(profileAttributeJson);
			}
			catch (WebException ex)
			{
				WebExceptionHandler(ex);
				return "";
			}
			catch (JsonReaderException ex)
			{
				EapExceptionHandler(ex);
				return "";
			}

			// checks profile attributes for a redirect link
			var redirect = "";
			foreach (var attribute in profileAttributes.Data.Devices)
			{
				if (attribute.Redirect != "0")
				{
					redirect = attribute.Redirect;
				}
			}
			return redirect;
		}

		/// <summary>
		/// Gets download link for EAP config from json and downloads it.
		/// </summary>
		/// <returns></returns>
		public string GetEapConfigString()
		{
			// adds profile ID to url containing json file, which in turn contains url to EAP config file download
			string generateEapUrl = $"https://cat.eduroam.org/user/API.php?action=generateInstaller&id=eap-config&lang=en&profile={profileId}";

			// contains json with eap config file download link
			GenerateEapConfig eapConfigInstance;
			try
			{
				// downloads json as string
				string generateEapJson = GetStringFromUrl(generateEapUrl);
				// converts json to GenerateEapConfig object
				eapConfigInstance = JsonConvert.DeserializeObject<GenerateEapConfig>(generateEapJson);
			}
			catch (WebException ex)
			{
				WebExceptionHandler(ex);
				return "";
			}
			catch (JsonReaderException ex)
			{
				EapExceptionHandler(ex);
				return "";
			}

			// gets url to EAP config file download from GenerateEapConfig object
			string eapConfigUrl = $"https://cat.eduroam.org/user/{eapConfigInstance.Data.Link}";

			// downloads and returns eap config file as string
			try
			{
				return GetStringFromUrl(eapConfigUrl);
			}
			catch (WebException ex)
			{
				WebExceptionHandler(ex);
				return "";
			}
		}

		/// <summary>
		/// Gets EAP-config file, either directly or after browser authentication.
		/// Prepares for redirect if no EAP-config.
		/// </summary>
		/// <returns>EapConfig object.</returns>
		public EapConfig DownloadEapConfig()
		{
			// checks if user has selected an institution and/or profile
			if (string.IsNullOrEmpty(profileId))
			{
				MessageBox.Show("Please select an institution and/or a profile.",
					"Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null; // exits function if no institution/profile selected
			}

			// checks for redirect link in profile attributes
			string redirect = GetProfileAttributes();
			// eap config file as string
			string eapString;

			// if no redirect link
			if (string.IsNullOrEmpty(redirect))
			{
				// gets eap config file directly
				eapString = GetEapConfigString();
			}
			// if Let's Wifi redirect
			else if (redirect.Contains("#letswifi"))
			{
				// get eap config file from browser authenticate
				eapString = OAuth.BrowserAuthenticate(redirect);
				// return focus to application
				frmParent.Activate();
			}
			// if other redirect
			else
			{
				// makes redirect link accessible in parent form
				frmParent.RedirectUrl = redirect;
				return null;
			}

			// if not empty, creates and returns EapConfig object from Eap string
			if (string.IsNullOrEmpty(eapString))
			{
				return null;
			}

			try
			{
				// if not empty, creates and returns EapConfig object from Eap string
				return ConnectToEduroam.GetEapConfig(eapString);
			}
			catch (XmlException ex)
			{
				EapExceptionHandler(ex);
				return null;
			}
		}

		/// <summary>
		/// Handles web exceptions that occur if user loses an existing connection.
		/// </summary>
		/// <param name="ex">WebException.</param>
		private void WebExceptionHandler(WebException ex)
		{
			HideControls();
			lblError.Text = "Couldn't connect to the server.\n\n"
							+ "Make sure that you are connected to the internet, then try again.";
			lblError.Visible = true;
			MessageBox.Show("It seems like you have lost your internet connection. Reconnect and try again.\n"
							+ "Exception: " + ex.Message, "eduroam - Exception",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>
		/// Handles exceptions related to deserializing JSON files and corrupted XML files.
		/// </summary>
		/// <param name="ex">Exception.</param>
		private void EapExceptionHandler(Exception ex)
		{
			MessageBox.Show("The selected institution or profile is not supported. " +
							"Please select a different institution or profile.\n"
							+ "Exception: " + ex.Message, "eduroam - Exception",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
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
