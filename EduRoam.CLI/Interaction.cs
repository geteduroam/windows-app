using EduRoam.Localization;

namespace EduRoam.CLI
{
    public static class Interaction
    {
        public static bool GetConfirmation()
        {
            Console.Write($"{Resources.AreYouSure} ({Resources.IsSure.ToLower()}/{Resources.NotSure.ToUpper()})");

            var choice = Console.ReadLine() ?? Resources.NotSure.ToUpper();

            return (choice.Trim().ToString().Equals(Resources.IsSure, StringComparison.CurrentCultureIgnoreCase));
        }

        public static string GetYesNoText(bool status)
        {
            return status ? Resources.Yes : Resources.No;
        }
    }
}
