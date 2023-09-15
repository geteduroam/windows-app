using System;

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

        public static void WriteWarning(string value, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(value, args);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteStatusIf(Func<bool> showIf, string value, params object[] args)
        {
            if (showIf())
            {
                WriteStatus(value, args);
            }
        }

        public static void WriteStatusIf(bool show, string value, params object[] args)
        {
            if (show)
            {
                WriteStatus(value, args);
            }
        }

        public static void WriteStatus(string value, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(value, args);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
