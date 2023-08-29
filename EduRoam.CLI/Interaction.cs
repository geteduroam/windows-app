using SharedResources = EduRoam.Localization.Resources;

namespace EduRoam.CLI
{
    public static class Interaction
    {
        public static bool GetConfirmation()
        {
            Console.Write($"{SharedResources.AreYouSure} ({SharedResources.IsSure.ToLower()}/{SharedResources.NotSure.ToUpper()})");

            var choice = Console.ReadLine() ?? SharedResources.NotSure.ToUpper();

            return (choice.Trim().ToString().Equals(SharedResources.IsSure, StringComparison.CurrentCultureIgnoreCase));
        }

        public static string GetYesNoText(bool status)
        {
            return status ? SharedResources.Yes : SharedResources.No;
        }
    }
}
