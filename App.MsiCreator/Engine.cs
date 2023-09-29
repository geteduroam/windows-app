using App.MsiCreator.Commands;

using System.CommandLine;
using System.Threading.Tasks;

namespace App.MsiCreator
{
    public class Engine
    {
        private readonly RootCommand rootCommand;

        public Engine()
        {
            this.rootCommand = new RootCommand("Create Msi (.msi) package");
            this.rootCommand.AddCommand(new CreateMsi().GetCommand());
        }

        public async Task Run(string[] args)
        {
            await this.rootCommand.InvokeAsync(args);
        }
    }
}
