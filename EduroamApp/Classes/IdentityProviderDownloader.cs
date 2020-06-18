using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Newtonsoft.Json;

namespace EduroamApp
{
	class IdentityProviderDownloader
	{
		/// <summary>
		/// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
		/// </summary>
		/// <exception cref="EduroamAppUserError">description</exception>
		public static List<IdentityProvider> GetAllIdProviders()
		{
			// url for json containing all identity providers/institutions
			const string allIdentityProvidersUrl = "https://cat.eduroam.org/user/API.php?action=listAllIdentityProviders&lang=en";
			try
			{
				string idProviderJson = GetStringFromUrl(allIdentityProvidersUrl);
				List<IdentityProvider> identityProviders = JsonConvert.DeserializeObject<List<IdentityProvider>>(idProviderJson);

				if (identityProviders.Count <= 0)
				{
					throw new EduroamAppUserError("", "Institutions couldn't be read from JSON file.");
				}
				return identityProviders;
			}
			catch (WebException ex)
			{
				throw new EduroamAppUserError("", GetWebExceptionString(ex));
			}

		}
		/// <summary>
		/// Gets all profiles associated with a identity provider ID.
		/// </summary>
		/// <returns>identity provider profile object containing all profiles for given provider</returns>
		/// <exception cref="EduroamAppUserError">description</exception>
		public static IdentityProviderProfile GetIdentityProviderProfiles(int idProviderId)
		{

			// adds institution id to url
			string profilesUrl = $"https://cat.eduroam.org/user/API.php?action=listProfiles&id={idProviderId}&lang=en";

			try
			{
				// downloads json file as string
				string profilesJson = GetStringFromUrl(profilesUrl);
				// gets identity provider profile from json
				IdentityProviderProfile idProviderProfiles = JsonConvert.DeserializeObject<IdentityProviderProfile>(profilesJson);
				return idProviderProfiles;
			}
			catch (WebException ex)
			{
				throw new EduroamAppUserError("", GetWebExceptionString(ex));
			}
			catch (JsonReaderException ex)
			{
				throw new EduroamAppUserError("", GetJsonExceptionString(ex));
			}
		}

		/// <summary>
		/// Gets profile attributes for a given profile ID.
		/// </summary>
		/// <returns>Profile Attributes</returns>
		public static IdProviderProfileAttributes GetProfileAttributes(string profileId)
		{
			// download attribute in json format
			string profileAttributeUrl = $"https://cat.eduroam.org/user/API.php?action=profileAttributes&id={profileId}&lang=en";
			string profileAttributeJson = GetStringFromUrl(profileAttributeUrl);

			// deserialize json to get profilattributes
			IdProviderProfileAttributes profileAttributes;
			profileAttributes = JsonConvert.DeserializeObject<IdProviderProfileAttributes>(profileAttributeJson);

			return profileAttributes;
		}



		/// <summary>
		/// Gets download link for EAP config from json and downloads it.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="EduroamAppUserError">description</exception>
		public static string GetEapConfigString(string profileId)
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
				throw new EduroamAppUserError("", GetWebExceptionString(ex));
			}
			catch (JsonReaderException ex)
			{
				throw new EduroamAppUserError("", GetJsonExceptionString(ex));
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
				throw new EduroamAppUserError("", GetWebExceptionString(ex));
			}
		}

		/// <summary>
		/// Gets a json file as string from url.
		/// </summary>
		/// <param name="url">Url containing json file.</param>
		/// <returns>Json string.</returns>
		private static string GetStringFromUrl(string url)
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
		/// Produces error string for web exceptions that occur if user loses an existing connection.
		/// </summary>
		/// <param name="ex">WebException.</param>
		/// <returns>String containing error message meant for user to read.</returns>
		private static string GetWebExceptionString(WebException ex)
		{
			string error =   "Couldn't connect to the server.\n\n"
					+ "Make sure that you are connected to the internet, then try again.\n" +
					"Exception: " + ex.Message;
			return error;
		}


		/// <summary>
		/// Produces error string for exceptions related to deserializing JSON files and corrupted XML files.
		/// </summary>
		/// <param name="ex">Exception.</param>
		/// <returns>String containing error message meant for user to read.</returns>
		private static string GetJsonExceptionString(Exception ex)
		{
			string error = "The selected institution or profile is not supported. " +
							"Please select a different institution or profile.\n"
							+ "Exception: " + ex.Message;
			return error;
		}

	}

}
