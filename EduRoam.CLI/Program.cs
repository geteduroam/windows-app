// See https://aka.ms/new-console-template for more information
using EduRoam.CLI;

using SharedResources = EduRoam.Localization.Resources;

public class Program
{
    private static Engine Engine => new Engine();

    public static async Task Main(string[] args)
    {
        SharedResources.Culture = System.Globalization.CultureInfo.CurrentUICulture;

        await Engine.Run(args);
    }
}
