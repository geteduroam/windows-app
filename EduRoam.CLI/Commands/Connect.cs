using EduRoam.Connect;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public class Connect : ICommand
    {
        public static readonly string CommandName = "connect";

        public static readonly string CommandDescription = Resource.CommandDescriptionConnect;

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(async () =>
            {
                var connectTask = new ConnectTask();

                try
                {
                    var (connected, message) = await connectTask.ConnectAsync();

                    if (connected)
                    {
                        ConsoleExtension.WriteStatus(message);
                    }
                    else
                    {
                        ConsoleExtension.WriteError(message);
                    }

                }
                catch (EduroamAppUserException ex)
                {
                    // TODO, NICE TO HAVE: log the error
                    ConsoleExtension.WriteError(Resource.ErrorNoConnection, ex.UserFacingMessage);
                }

                catch (ArgumentException exc)
                {
                    ConsoleExtension.WriteError(exc.Message);
                }
                catch (ApiParsingException e)
                {
                    // Must never happen, because if the discovery is reached,
                    // it must be parseable. Logging has been done upstream.
                    ConsoleExtension.WriteError(Resource.ErrorApi);
                    ConsoleExtension.WriteError(e.Message, e.GetType().ToString());
                }
                catch (ApiUnreachableException)
                {
                    ConsoleExtension.WriteError(Resource.ErrorNoInternet);
                }

            });

            return command;
        }
    }
}
