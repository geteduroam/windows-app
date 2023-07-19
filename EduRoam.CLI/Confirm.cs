using EduRoam.Connect.Language;

namespace EduRoam.CLI
{
    public static class Confirm
    {
        public static bool GetConfirmation()
        {
            Console.Write($"{Resource.AreYouSure} ({Resource.IsSure.ToLower()}/{Resource.NotSure.ToUpper()})");

            var choice = Console.ReadLine() ?? Resource.NotSure.ToUpper();

            return (choice.Trim().ToString().Equals(Resource.IsSure, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
