// See https://aka.ms/new-console-template for more information
using EduRoam.CLI;
using EduRoam.Connect.Language;

public class Program
{
    private static Engine Engine => new Engine();

    public static async Task Main(string[] args)
    {
        Resources.Culture = System.Globalization.CultureInfo.CurrentUICulture;

        await Engine.Run(args);
    }
}
