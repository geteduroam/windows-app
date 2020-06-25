using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EduroamConfigure
{
	public class IdentityProviderParser
	{

		public static List<IdentityProvider> SortBySearch(List<IdentityProvider> providers, string searchString)
		{
			searchString = NormalizeString(searchString);
			// sort by prioritized criterias. first criteria most important
			List<IdentityProvider> sortedList = providers.OrderByDescending
				// name contains a word equal to the exact search string
				(p =>
					{
						foreach (string word in NormalizeString(p.Name).Split(' '))
						{
							if (string.Equals(word, searchString))
							{
									return true;
							}
						}
						return false;
					}
				// name starts with search string
				).ThenByDescending(p => NormalizeString(p.Name).StartsWith(searchString)
				// acronym for name contains searchstring
				).ThenByDescending(p => StringToAcronym(NormalizeString(p.Name)).Contains(searchString)
				// any word in name begins with search string
				).ThenByDescending(p =>
					{
						 foreach (string word in NormalizeString(p.Name).Split(' '))
						 {
							 if (word.StartsWith(searchString))
							 {
								 return true;
							 }
						 }
						 return false;
					}
				// search string can be found somewhere in the name
				).ThenByDescending(p => NormalizeString(p.Name).Contains(searchString)
			).ToList();

			return sortedList;
		}

		//removes accents and converts non-US character to US charactres (ø to o etc)
		private static string NormalizeString(string str)
		{
			string strippedString = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(str)).ToLowerInvariant();
			strippedString = strippedString.Replace("(", "");
			strippedString = strippedString.Replace(")", "");
			return strippedString;
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


		public static bool VerifyUsername(string username, string realm, bool strictRealm)
		{
			Regex rx;
			if (string.IsNullOrEmpty(realm)) {
				return VerifyUsernameGeneric(username);
			}
			if (strictRealm) {
				rx = new Regex($@"^([a-zA-Z0-9](?:[._-]?[a-zA-Z0-9]+)*)@{realm}$");
			}

			rx = new Regex($@"^([a-zA-Z0-9](?:[._-]?[a-zA-Z0-9]+)*)@(([a-zA-Z0-9]+[._-])*{realm})$");
			var match = rx.Match(username);
			return match.Success;
		}

		public static bool VerifyUsernameGeneric(string username)
		{
			Regex rx = new Regex(@"^([a-zA-Z0-9](?:[._-]?[a-zA-Z0-9]+)*)@([a-zA-Z0-9](?:[._-]?[a-zA-Z0-9]+)*\.[a-zA-Z0-9](?:[._-]?[a-zA-Z0-9]+)*)$");
			var match = rx.Match(username);
			return match.Success;
		}

		public static string GetBrokenRules(string username, string realm, bool strictRealm)
		{
			string ruleString = "";
			//checks that there is exactly one @ sign
			// positive lookahead require to find one @. Negative lookahead denies if string contains two (or more) @.
			Regex hasOneAt = new Regex(@"^(?=.*@.*)(?!.*@.*@).*$");
			if (!hasOneAt.Match(username).Success)
			{
				ruleString += "Username must contain exactly one @\n";
			}

			// if realm is specified
			if (!string.IsNullOrEmpty(realm))
			{
				// if strict realm is set then there can be no subrealms
				// if not strict: realm = eduroam.no will allow @pedkek.eduroam.no
				// if strict: has to be @eduroam.no
				if (strictRealm)
				{
					Regex endsWithRealm = new Regex($@"^.*@{realm}$");
					if (!endsWithRealm.Match(username).Success)
					{
						ruleString += $"Username must end with @{realm}\n";
					}
				}
				else
				{
					Regex endsWithRealm = new Regex($@"^.*[._\-@]{realm}$");
					if (!endsWithRealm.Match(username).Success)
					{
						ruleString += $"Username must end with {realm}\n";
					}
				}
			}

			// checks that special characters are not adjacent to each other
			Regex noAdjacentSpecialChars = new Regex(@"^(?!.*[._\-@]{2}.*).*$");
			if (!noAdjacentSpecialChars.Match(username).Success)
			{
				ruleString += "Characters such as [-.@_] can not be adjacent to each other\n";
			}


			//checks that username begins with a vald alue
			Regex validStart = new Regex(@"^[a-zA-Z0-9].*$");
			if (!validStart.Match(username).Success)
			{
				ruleString += "Username must begin with aphanumeric char\n";
			}

			//checks that username ends with a vald alue
			Regex validEnd = new Regex(@"^.*[a-zA-Z0-9]$");
			if (!validEnd.Match(username).Success)
			{
				ruleString += "Username must end with aphanumeric char\n";
			}

			return ruleString;
		}
	}
}
