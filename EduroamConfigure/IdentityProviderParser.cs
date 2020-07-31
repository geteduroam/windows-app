using DuoVia.FuzzyStrings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EduroamConfigure
{
    public static class IdentityProviderParser
    {
        /// <summary>
        /// Searches thourgh the list of providers, intended for user-facing search interfaces.
        /// </summary>
        /// <param name="providers">List of providers to query</param>
        /// <param name="searchString">Query string</param>
        /// <param name="limit">Number of results to return, reduce this for a speedup</param>
        /// <returns>List of providers ordered by match coefficient</returns>
        public static List<IdentityProvider> SortByQuery(List<IdentityProvider> providers, string searchString, int limit)
        {
            var query = NormalizeString(searchString);

            bool startsWithInv(string str, string query) =>
                str.StartsWith(query, StringComparison.InvariantCultureIgnoreCase);

            // TODO: add realms/domain as possible match

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
            string strippedString = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(str))
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
        /// </summary>
        /// <param name="username">user[@realm]</param>
        /// <param name="requiredRealm">the realm required for the username</param>
        /// <param name="noSubdomanInRealm">Wether to allow subdomains in the realm</param>
        /// <returns>nothing if no rules are broken, otherwise descriptions of rulse being broken</returns>
        public static IEnumerable<string> GetRulesBroken(string username, string requiredRealm, bool noSubdomanInRealm)
        {
            // TODO: rename, docstring, perhaps move?

            //checks that there is exactly one @ sign
            // positive lookahead required to find one @. Negative lookahead denies if string contains two (or more) @.
            Regex hasOneAt = new Regex(@"^(?=.*@.*)(?!.*@.*@).*$");
            if (!hasOneAt.Match(username).Success)
                yield return "Username must contain exactly one @";
            
            // if realm is specified
            if (!string.IsNullOrEmpty(requiredRealm))
            {
                /* 
                Regex hasOneAt = new Regex(@"^(?=.*@.*)(?!.*@.*@).*$");
                if (!hasOneAt.Match(username).Success)
                    yield return "Username must contain exactly one @";
                */

                if (noSubdomanInRealm)
                {
                    Regex endsWithRealm = new Regex($@"^.*@{requiredRealm}$");
                    if (!endsWithRealm.Match(username).Success)
                        yield return $"Username must end with @{requiredRealm}";
                }
                else
                {
                    Regex endsWithRealm = new Regex($@"^.*[._\-@]{requiredRealm}$");
                    if (!endsWithRealm.Match(username).Success)
                        yield return $"Username must end with {requiredRealm}";
                }
            }

            // checks that special characters are not adjacent to each other
            Regex noAdjacentSpecialChars = new Regex(@"^(?!.*[._\-@]{2}.*).*$");
            if (!noAdjacentSpecialChars.Match(username).Success)
                yield return "Characters such as [-.@_] can not be adjacent to each other";

            //checks that username begins with a vald alue
            Regex validStart = new Regex(@"^[a-zA-Z0-9].*$");
            if (!validStart.Match(username).Success)
                yield return "Username must begin with aphanumeric char";

            //checks that username ends with a vald alue
            Regex validEnd = new Regex(@"^.*[a-zA-Z0-9]$");
            if (!validEnd.Match(username).Success)
                yield return "Username must end with aphanumeric char";
        }
    }
}
