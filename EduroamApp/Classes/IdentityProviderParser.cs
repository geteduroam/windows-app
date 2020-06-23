using System;
using System.Collections.Generic;
using System.Linq;
using System.Device.Location;
using System.Globalization;
using System.Text;

namespace EduroamApp
{
	class IdentityProviderParser
	{

		public static List<IdentityProvider> SortBySearch(List<IdentityProvider> providers, string searchString)
		{
			searchString = NormalizeString(searchString);
			List<IdentityProvider> sortedList = providers.OrderByDescending(
				p => NormalizeString(p.Name).StartsWith(searchString)
				).ThenByDescending(
				p => {
					foreach (String word in NormalizeString(p.Name).Split(' '))
					{
						if (word.StartsWith(searchString))
						{
								return true;
						}
					}
					return false;
				}
				).ThenByDescending(p => StringToAcronym(NormalizeString(p.Name)).Contains(searchString)
				).ThenByDescending(p => NormalizeString(p.Name).Contains(searchString)
			).ToList();

			return sortedList;
		}

		private static string NormalizeString(string str)
		{
			string normalized = str.Normalize(NormalizationForm.FormD);


			StringBuilder resultBuilder = new StringBuilder();
			foreach (var character in normalized)
			{
				UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
				if (category == UnicodeCategory.LowercaseLetter
					|| category == UnicodeCategory.UppercaseLetter
					|| category == UnicodeCategory.SpaceSeparator)
					resultBuilder.Append(character);
			}
			return resultBuilder.ToString().ToLowerInvariant();
		}

		private static string StringToAcronym(string str)
		{
			string resultString = "";
			foreach (String word in str.ToLower().Split(' '))
			{
				if (word.Count() > 0)
				{
					resultString += word[0];
				}
			}
			return resultString;
		}


	}
}
