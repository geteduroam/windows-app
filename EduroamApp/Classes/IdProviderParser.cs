using System;
using System.Collections.Generic;
using System.Linq;
using System.Device.Location;
using System.Globalization;

namespace EduroamApp
{
	class IdProviderParser
	{
		/// <summary>
		/// Finds the identity provider that is closest to the given coordinates
		/// </summary>
		/// <param name="instList">List of all institutions/identity providrs.</param>
		/// <returns>The identity provider closest to coordinates.</returns>
		/// <exception cref="EduroamAppUserError">description</exception>
		public static IdentityProvider GetClosestIdProvider(List<IdentityProvider> instList, GeoCoordinate userCoord)
		{
			if (userCoord.IsUnknown)
			{
				throw new EduroamAppUserError("", "Found no coordinates for machine");

			}
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
			return closestInst;
		}

		/// <summary>
		/// Compares institution coordinates with local machines coordinates and gets the country code from the closest institution.
		/// If no closest insitution is found, region and language settings for the machine is used instead
		/// </summary>
		/// <param name="instList">List of all institutions.</param>
		/// <param name="userCoord"> Coordinates to compare with institution coordinates.</param>
		/// <returns>Country Code of closest institution.</returns>
		public static string GetClosestCountryCode(List<IdentityProvider> instList, GeoCoordinate userCoord)
		{

			//Reads country code from local machine if no coordinates found
			try
			{
				return GetClosestIdProvider(instList, userCoord).Country;
			}
			catch (EduroamAppUserError ex)
			{

			}

			// gets country as set in Settings
			// https://stackoverflow.com/questions/8879259/get-current-location-as-specified-in-region-and-language-in-c-sharp
			var regKeyGeoId = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\International\Geo");
			var geoID = (string)regKeyGeoId.GetValue("Nation");
			var allRegions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.ToString()));
			var regionInfo = allRegions.FirstOrDefault(r => r.GeoId == Int32.Parse(geoID));
			return regionInfo.TwoLetterISORegionName;
		}

		/// <summary>
		/// Turns list of identity providers into a list containing all countries found in the list
		/// </summary>
		/// <param name="instList">List of all institutions/identity providrs.</param>
		/// <returns>List of all countries found.</returns>
		public static List<Country> GetCountries(List<IdentityProvider> instList)
		{
			List<Country> countries = new List<Country>();
			List<string> distinctCountryCodes = instList.Select(provider => provider.Country)
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
			return countries;
		}


		/// <summary>
		/// Gets redirect link from profile's attributes
		/// </summary>
		/// <returns>Redirect link, if exists.</returns>
		public static string getRedirect(IdProviderProfileAttributes attributes)
		{
			// checks profile attributes for a redirect link
			var redirect = "";
			foreach (var attribute in attributes.Data.Devices)
			{
				if (attribute.Redirect != "0")
				{
					redirect = attribute.Redirect;
				}
			}
			return redirect;
		}
	}
}
