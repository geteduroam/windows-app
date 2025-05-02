using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EduRoam.Connect
{
    public static class Extensions
    {
        public static List<string> AsListItem(this string value)
        {
            return new List<string>() { value };
        }

        // When Windows makes a Wi-Fi profile, it formats the 8 bit hexits
        // lower case with a space after every hexit, including the last one
        // Formatting it this way does not seem necessary for the profile to work,
        // but we do so anyway in order to minimize potential problems
        public static string FormatHexString(this string thumb)
        {
            var value = Regex.Replace(thumb, " ", "");
            value = Regex.Replace(value, ".{2}", "$0 ");
            return value.ToLowerInvariant();
        }
    }
}
