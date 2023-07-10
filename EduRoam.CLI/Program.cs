// See https://aka.ms/new-console-template for more information
using EduRoam.CLI;

class Program
{
    private static Engine Engine => new Engine();

    static void Main(string[] args)
    {
        Engine.Run(args);
    }
}
