using System;

namespace EduRoam.CLI
{
    internal static class Input
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>Based on https://stackoverflow.com/a/3404522</remarks>
        internal static string ReadPassword()
        {
            var pass = string.Empty;
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass.Remove(pass.Length - 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            } while (keyInfo.Key != ConsoleKey.Enter);
            Console.WriteLine();

            return pass;
        }
    }
}
