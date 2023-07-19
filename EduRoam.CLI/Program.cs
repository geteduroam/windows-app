﻿// See https://aka.ms/new-console-template for more information
using EduRoam.CLI;

public class Program
{
    private static Engine Engine => new Engine();

    public static async Task Main(string[] args)
    {
        EduRoam.Connect.Resource.Culture = System.Globalization.CultureInfo.CurrentUICulture;

        await Engine.Run(args);
    }
}
