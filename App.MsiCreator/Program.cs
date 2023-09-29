using System.Threading.Tasks;

namespace App.MsiCreator
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var engine = new Engine();
            await engine.Run(args);
        }
    }
}
