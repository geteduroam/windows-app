using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EduroamConfigure
{
	public class IdentityProviderParser
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

		//removes accents and converts non-US character to US charactres (ø to o etc)
		private static string NormalizeString(string str)
		{
			return Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(str)).ToLowerInvariant();
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
