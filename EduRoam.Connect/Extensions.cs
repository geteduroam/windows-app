namespace EduRoam.Connect
{
    public static class Extensions
    {
        public static List<string> AsListItem(this string value)
        {
            return new List<string>() { value };
        }
    }
}
