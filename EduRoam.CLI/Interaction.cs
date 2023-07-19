using EduRoam.Connect.Language;

namespace EduRoam.CLI
{
    public static class Interaction
    {
        public static bool GetConfirmation()
        {
            Console.Write($"{Resource.AreYouSure} ({Resource.IsSure.ToLower()}/{Resource.NotSure.ToUpper()})");

            var choice = Console.ReadLine() ?? Resource.NotSure.ToUpper();

            return (choice.Trim().ToString().Equals(Resource.IsSure, StringComparison.CurrentCultureIgnoreCase));
        }

        public static string GetYesNoText(bool status)
        {
            return status ? Resource.Yes : Resource.No;
        }
    }
}
