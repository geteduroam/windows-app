using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ManagedNativeWifi;
using System.Net;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Device.Location;

namespace EduroamApp
{
	public partial class frmDownload : Form
	{
		readonly frmParent frmParent; // makes parent form accessible from this class
		List<IdentityProvider> identityProviders; // list containing all identity providers
		IdentityProviderProfile idProviderProfiles; // list containing all profiles of an identity provider
		int idProviderId; // id of selected institution
		string profileId; // id of selected institution profile

		public frmDownload(frmParent parentInstance)
		{
			// gets parent form instance
			frmParent = parentInstance;
			InitializeComponent();
		}

		private async void frm3_Load(object sender, EventArgs e)
		{
			// displays loading animation while fetching list of institutions
			lblCountry.Visible = false;
			lblInstitution.Visible = false;
			lblSelectProfile.Visible = false;
			cboCountry.Visible = false;
			cboInstitution.Visible = false;
			cboProfiles.Visible = false;
			tlpLoading.Visible = true;

			// async method to get list of institutions
			bool getInstSuccess = await Task.Run(() => GetAllInstitutions());

			if (getInstSuccess && identityProviders.Count > 0)
			{
				lblCountry.Visible = true;
				lblInstitution.Visible = true;
				lblSelectProfile.Visible = true;
				cboCountry.Visible = true;
				cboInstitution.Visible = true;
				cboProfiles.Visible = true;
				tlpLoading.Visible = false;

				PopulateCountries();

				frmParent.BtnNextEnabled = true;
			}
			else
			{
				tlpLoading.Visible = false;
				lblError.Visible = true;
			}
		}

		public bool GetAllInstitutions()
		{
			// url for json containing all identity providers / institutions
			const string allIdentityProvidersUrl = "https://cat.eduroam.org/user/API.php?action=listAllIdentityProviders&lang=en";

			// json file as string
			string idProviderJson;
			try
			{
				idProviderJson = UrlToJson(allIdentityProvidersUrl);
			}
			catch (WebException ex)
			{
				MessageBox.Show("Couldn't fetch identity provider list. \nException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			// gets list of identity providers from json file
			identityProviders = JsonConvert.DeserializeObject<List<IdentityProvider>>(idProviderJson);

			return true;
		}

		private void PopulateCountries()
		{
			// adds countries to combobox
			cboCountry.Items.AddRange(identityProviders.OrderBy(provider => provider.Country)
				.Select(provider => provider.Country)
				.Distinct()
				.ToArray());

			// finds the country geographically closest to the user and selects it by default
			try
			{
				string closestCountry = GetClosestInstitution(identityProviders);
				cboCountry.SelectedIndex = cboCountry.FindStringExact(closestCountry);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Couldn't get country \nException: " + ex.Message);
			}
		}

		private void cboCountry_SelectedIndexChanged(object sender, EventArgs e)
		{
			// clear combobox
			cboInstitution.Items.Clear();
			cboProfiles.Items.Clear();
			// clear selected profile
			profileId = null;

			// adds identity providers from selected country to combobox
			cboInstitution.Items.AddRange(identityProviders.Where(provider => provider.Country == cboCountry.Text)
											.OrderBy(provider => provider.Title).Select(provider => provider.Title).ToArray());
		}

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

			// json file as string
			string profilesJson;
			try
			{
				profilesJson = UrlToJson(profilesUrl);
			}
			catch (WebException ex)
			{
				MessageBox.Show("Couldn't fetch identity provider profiles.\nException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// gets identity provider profile from json
			idProviderProfiles = JsonConvert.DeserializeObject<IdentityProviderProfile>(profilesJson);

			// if an identity provider has more than one profile, add to combobox
			if (idProviderProfiles.Data.Count > 1)
			{
				// enable combobox
				cboProfiles.Enabled = true;
				// enable label
				lblSelectProfile.Enabled = true;
				// add profiles to combobox
				cboProfiles.Items.AddRange(idProviderProfiles.Data.Select(profile => profile.Display).ToArray());
			}
			else
			{
				// gets the only profile id
				profileId = idProviderProfiles.Data.Single().Id;
				// disable combobox
				cboProfiles.Enabled = false;
				// disable label
				lblSelectProfile.Enabled = false;
			}
		}

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
		public string UrlToJson(string url)
		{
			// downloads json file from url as string
			using (WebClient client = new WebClient())
			{
				string jsonString = client.DownloadString(url);
				return jsonString;
			}
		}

		/// <summary>
		/// Compares institution coordinates with user's coordinates and gets the closest institution.
		/// </summary>
		/// <param name="instList">List of all institutions.</param>
		/// <returns>Country of closest institution.</returns>
		public string GetClosestInstitution(List<IdentityProvider> instList)
		{
			// gets GeoCoordinateWatcher from parent form
			GeoCoordinateWatcher watcher = frmParent.GetWatcher();
			// user's coordinates
			GeoCoordinate myCoord = watcher.Position.Location;
			// institution's coordinates
			GeoCoordinate instCoord = new GeoCoordinate();
			// closest institution
			IdentityProvider closestInst = new IdentityProvider();
			// shortest distance
			double shortestDistance = double.MaxValue;

			// loops through all institutions' coordinates and compares them with current shortest distance
			foreach (IdentityProvider inst in instList)
			{
				if (inst.MyGeo != null) // excludes if geo property not set
				{
					// gets lat and long
					instCoord.Latitude = inst.MyGeo.First().Lat;
					instCoord.Longitude = inst.MyGeo.First().Lon;
					// gets current distance
					double currentDistance = myCoord.GetDistanceTo(instCoord);
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

		public string GetEapConfigString()
		{
			// eap config file
			string eapString = "";

			// adds profile ID to url containing json file, which in turn contains url to EAP config file download
			string generateEapUrl = $"https://cat.eduroam.org/user/API.php?action=generateInstaller&id=eap-config&lang=en&profile={profileId}";

			// contains json with eap config file download link
			GenerateEapConfig eapConfigInstance;

			try
			{
				// downloads json as string
				string generateEapJson = UrlToJson(generateEapUrl);
				// converts json to GenerateEapConfig object
				eapConfigInstance = JsonConvert.DeserializeObject<GenerateEapConfig>(generateEapJson);
			}
			catch (WebException ex)
			{
				MessageBox.Show("Couldn't fetch Eap Config generate.\n" +
								"Exception: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return eapString;
			}
			catch (JsonReaderException ex)
			{
				MessageBox.Show("No supported EAP types found for this profile.\n" +
								"Exception: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return eapString;
			}

			// gets url to EAP config file download from GenerateEapConfig object
			string eapConfigUrl = $"https://cat.eduroam.org/user/{eapConfigInstance.Data.Link}";

			// gets eap config file as string
			try
			{
				eapString = UrlToJson(eapConfigUrl);
			}
			catch (WebException ex)
			{
				MessageBox.Show("Couldn't fetch Eap Config file.\n" +
								"Exception: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return eapString;
		}

		public uint ConnectWithDownload()
		{
			// checks if user has selected an institution and/or profile
			if (string.IsNullOrEmpty(profileId))
			{
				MessageBox.Show("Please select an institution and/or a profile.",
					"Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return 0; // exits function if no institution/profile selected
			}

			string eapString = GetEapConfigString();
			uint eapType = 0;
			string instId = null;

			try
			{
				eapType = ConnectToEduroam.Setup(eapString);
				instId = ConnectToEduroam.GetInstId(eapString);
			}
			catch (ArgumentException argEx)
			{
				if (argEx.Message == "interfaceId")
				{
					MessageBox.Show("Could not establish a connection through your computer's wireless network interface. \n" +
									"Please go to Control Panel -> Network and Internet -> Network Connections to make sure that it is enabled.",
									"Network interface error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			// makes the institution Id accessible from parent form
			frmParent.LblInstText = instId;
			return eapType;
		}

	}
}
