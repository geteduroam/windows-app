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
	public partial class frm3 : Form
	{
		frmParent frmParent;
		List<IdentityProvider> identityProviders; // list containing all identity providers
		IdentityProviderProfile idProviderProfiles; // list containing all profiles of an identity provider
		int idProviderId; // id of selected institution
		string profileId; // id of selected institution profile

		public frm3(frmParent parentInstance)
		{
			frmParent = parentInstance;
			InitializeComponent();
		}

		private void frm3_Load(object sender, EventArgs e)
		{
			// url for json containing all identity providers / institutions
			string allIdentityProvidersUrl = "https://cat.eduroam.org/user/API.php?action=listAllIdentityProviders&lang=en";

			// json file as string
			string idProviderJson = "";
			try
			{
				idProviderJson = urlToJson(allIdentityProvidersUrl);
			}
			catch (WebException ex)
			{
				MessageBox.Show("Couldn't fetch identity provider list. \nException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// gets list of identity providers from json file
			identityProviders = JsonConvert.DeserializeObject<List<IdentityProvider>>(idProviderJson);
			// adds countries to combobox
			cboCountry.Items.AddRange(identityProviders.OrderBy(provider => provider.country).Select(provider => provider.country).Distinct().ToArray());

			// finds the country geographically closest to the user and selects it by default
			try
			{
				string closestCountry = GetClosestInstitution(identityProviders);
				cboCountry.SelectedIndex = cboCountry.FindStringExact(closestCountry);
			}
			catch (System.Exception)
			{
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
			cboInstitution.Items.AddRange(identityProviders.Where(provider => provider.country == cboCountry.Text)
											.OrderBy(provider => provider.title).Select(provider => provider.title).ToArray());
		}

		private void cboInstitution_SelectedIndexChanged(object sender, EventArgs e)
		{
			// clear combobox
			cboProfiles.Items.Clear();
			// clear selected profile
			profileId = null;

			// gets id of institution selected in combobox
			idProviderId = identityProviders.Where(x => x.title == cboInstitution.Text).Select(x => x.id).First();
			// adds institution id to url
			string profilesUrl = $"https://cat.eduroam.org/user/API.php?action=listProfiles&id={idProviderId}&lang=en";

			// json file as string
			string profilesJson = "";
			try
			{
				profilesJson = urlToJson(profilesUrl);
			}
			catch (WebException ex)
			{
				MessageBox.Show("Couldn't fetch identity provider profiles.\nException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// gets identity provider profile from json
			idProviderProfiles = JsonConvert.DeserializeObject<IdentityProviderProfile>(profilesJson);

			// if an identity provider has more than one profile, add to combobox
			if (idProviderProfiles.data.Count > 1)
			{
				// enable combobox
				cboProfiles.Enabled = true;
				// enable label
				lblSelectProfile.Enabled = true;
				// add profiles to combobox
				cboProfiles.Items.AddRange(idProviderProfiles.data.Select(profile => profile.display).ToArray());
			}
			else
			{
				// gets the only profile id
				profileId = idProviderProfiles.data.Single().id;
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
				profileId = idProviderProfiles.data.Where(profile => profile.display == cboProfiles.Text).Select(x => x.id).Single();
			}
		}

		/// <summary>
		/// Gets a json file as string from url.
		/// </summary>
		/// <param name="url">Url containing json file.</param>
		/// <returns>Json string.</returns>
		public string urlToJson(string url)
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
			// current distance
			double currentDistance;
			// closest institution
			IdentityProvider closestInst = new IdentityProvider();
			// shortest distance
			double shortestDistance = double.MaxValue;

			// loops through all institutions' coordinates and compares them with current shortest distance
			foreach (IdentityProvider inst in instList)
			{
				if (inst.geo != null) // excludes if geo property not set
				{
					// gets lat and long
					instCoord.Latitude = inst.geo.First().lat;
					instCoord.Longitude = inst.geo.First().lon;
					// gets distance
					currentDistance = myCoord.GetDistanceTo(instCoord);
					// compares with current shortest distance
					if (currentDistance < shortestDistance)
					{
						shortestDistance = currentDistance;
						closestInst = inst;
					}
				}
			}

			// returns country of institution closest to user
			return closestInst.country;
		}
	}
}
