using System;
using System.Collections.Generic;
using System.Linq;
using System.Device.Location;
using System.Globalization;

namespace EduroamApp
{
	class IdentityProviderParser
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

				if (!IsNullOrEmptyList(inst.Geo)) // excludes if geo property not set
				{

					// check all coordinates for each institute
					foreach (Geo instGeo in inst.Geo)
					{
						// store to GeoCoordinate object
						instCoord.Latitude = instGeo.Lat;
						instCoord.Longitude = instGeo.Lon;
						// gets current distance
						double currentDistance = userCoord.GetDistanceTo(instCoord);
						// compares with shortest distance
						if (currentDistance < shortestDistance)
						{
							// sets the current distance as the shortest dstance
							shortestDistance = currentDistance;
							// sets inst with shortest distance to be the closest institute
							closestInst = inst;
						}
					}
				}
			}
			return closestInst;
		}

		/// <summary>
		/// Determines whether the collection is null or contains no elements.
		/// </summary>
		/// <typeparam name="T">The IEnumerable type.</typeparam>
		/// <param name="enumerable">The enumerable, which may be null or empty.</param>
		/// <returns>
		///     <c>true</c> if the IEnumerable is null or empty; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsNullOrEmptyList<T>(IEnumerable<T> enumerable)
		{
			if (enumerable == null)
			{
				return true;
			}
			/* If this is a list, use the Count property for efficiency.
			 * The Count property is O(1) while IEnumerable.Count() is O(N). */
			var collection = enumerable as ICollection<T>;
			if (collection != null)
			{
				return collection.Count < 1;
			}
			return !enumerable.Any();
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
			catch (EduroamAppUserError)
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
	}
}
