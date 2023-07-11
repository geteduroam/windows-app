namespace EduRoam.Connect
{
    public static class ConsoleExtension
    {
        public static void WriteError(string value, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(value, args);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteStatus(string value, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(value, args);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
