using EduRoam.Connect;

namespace EduRoam.CLI
{
    public static class Confirm
    {
        public static bool GetConfirmation()
        {
            Console.Write($"{Resource.AreYouSure} ({Resource.IsSure.ToLower()}/{Resource.NotSure.ToUpper()})");

            var choice = Console.ReadKey();

            return (choice.KeyChar.ToString().Equals(Resource.IsSure, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
