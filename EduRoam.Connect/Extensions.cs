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

        public static string ToHexBinary(this string thumb)
        {
            var value = Regex.Replace(thumb, " ", "");
            value = Regex.Replace(value, ".{2}", "$0 ");
            value = value.ToUpperInvariant();
            return value.Trim();
        }
    }


}
