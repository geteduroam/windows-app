using DuoVia.FuzzyStrings;

using EduRoam.Localization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EduRoam.Connect.Identity
{
    public static class IdentityProviderParser
    {
        /// <summary>
        /// Searches thourgh the list of providers, intended for user-facing search interfaces.
        /// </summary>
        /// <param name="providers">List of providers to query</param>
        /// <param name="searchString">Query string</param>
        /// <param name="limit">Maximum number of results (999 by default) to return, reduce this for a speedup</param>
        /// <returns>List of providers ordered by match coefficient</returns>
        public static IEnumerable<IdentityProvider> SortByQuery(IEnumerable<IdentityProvider> providers, string? searchString, int limit = 999)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return providers;
            }
            var query = NormalizeString(searchString);

            static bool startsWithInv(string str, string query) =>
                str.StartsWith(query, StringComparison.InvariantCultureIgnoreCase);

            // NICE TO HAVE: add realms/domain as possible match

            // Lexically sort by prioritized criterias.
            var sortedList = providers
                // Precompute compute the normalized name
                .Select(provider => (nname: NormalizeString(provider.Name), provider))

                // name contains a word equal to the exact search string
                .OrderByDescending(p => p.nname.Split(null).Contains(query))

                // name starts with search string
                .ThenByDescending(p => startsWithInv(p.nname, query))

                // acronym for name contains searchstring
                .ThenByDescending(p => StringToAcronym(p.nname).Contains(query))

                // any word in name begins with search string
                .ThenByDescending(p => p.nname.Split(null).Any(word => startsWithInv(word, query)))

                // search string can be found somewhere in the name
                .ThenByDescending(p => p.nname.Contains(query))

                // Fuzzy match string
                .ThenByDescending(p => p.nname.FuzzyMatch(query))

                // due to all of this being evaluated lazily, this is a major speedup
                .Take(limit)

                // Strip the normalized name
                .Select(p => p.provider)

                .ToList();

            return sortedList;
        }

        /// <summary>
        /// removes accents, casing and converts non-US character to US characters (ø to o etc)
        /// </summary>
        private static string NormalizeString(string str)
        {
            // TODO: perhaps allow non-us characters?
            var strippedString = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(str))
                .ToUpperInvariant()
                .Replace("-", " ")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("(", "")
                .Replace(")", "");
            return strippedString;
        }

        private static string StringToAcronym(string str)
        {
            return string.Join("", str
                .ToUpperInvariant()
                .Split(null) // whitespace
                .Where(part => part.Any())
                .Select(word => word[0]));
        }

        /// <summary>
        /// Returns a sequence of broken formatting rules for the username.
        ///
        /// if noSubdomanInRealm is set then there can be no subrealms
        /// otherwise: requiredRealm = 'eduroam.no' will allow @pedkek.eduroam.no
        /// if noSubdomanInRealm: must be exactly @eduroam.no
        ///
        /// Rules from https://github.com/GEANT/CAT/blob/master/tutorials/MappingCATOptionsIntoSupplicantConfig.md#verify-user-input-to-contain-realm-suffix-checkbox
        /// </summary>
        /// <param name="username">user[@realm]</param>
        /// <param name="requiredRealm">the realm required for the username, empty for any realm, null for no realm needed</param>
        /// <param name="noSubDomainInRealm">Wether to allow subdomains in the realm</param>
        /// <returns>nothing if no rules are broken, otherwise descriptions of rulse being broken</returns>
        public static IEnumerable<string> GetRulesBrokenOnUsername(string? username, string? requiredRealm, bool noSubDomainInRealm)
        {
            // TODO: perhaps move this function?

            // If no username given, or no realm is required, do no sanity check
            if (string.IsNullOrWhiteSpace(username) || requiredRealm == null)
            {
                yield break;
            }

            // checks that special characters are not adjacent to each other
            if (username.Contains("..") || username.Contains(".@") || username.Contains("@."))
            {
                yield return Resources.ErrorCredentialsSpecialCharacters;
            }
            else
            {
                // there a no two @@ adjacent, but there should be only one @ at all
                var index = username.IndexOf('@');
                if (index == -1 || index != username.LastIndexOf('@'))
                {
                    yield return Resources.ErrorCredentialsAtChar;
                }
            }

            // if realm is specified
            if (string.IsNullOrEmpty(requiredRealm))
            {
                // no specific realm set, only check that the username does not end with dot or whitespace
                var userNameEnding = username[username.Length - 1];
                if (userNameEnding == '.' || userNameEnding == ' ')
                {
                    yield return Resources.ErrorCredentialsEndsWith;
                }
            }
            else
            {
                // check that username ends with the specified realm
                if (
                    !username.EndsWith("@" + requiredRealm, StringComparison.Ordinal)
                    && (noSubDomainInRealm || !username.EndsWith("." + requiredRealm, StringComparison.Ordinal)))
                {
                    yield return noSubDomainInRealm
                        ? string.Format(Resources.ErrorCredentialsEndWithRealmNoSubdomain, requiredRealm)
                        : string.Format(Resources.ErrorCredentialsEndWithRealm, requiredRealm);
                }
            }
        }
    }
}
