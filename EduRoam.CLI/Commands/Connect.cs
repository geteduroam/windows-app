using EduRoam.Connect;
using EduRoam.Connect.Exceptions;
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
                    var connected = await connectTask.ConnectAsync();

                    if (connected)
                    {
                        ConsoleExtension.WriteStatus(Resource.Connected);
                    }
                    else
                    {
                        var eapConfigTask = new GetEapConfigTask();

                        var eapConfig = await eapConfigTask.GetEapConfigAsync();

                        if (eapConfig == null)
                        {
                            ConsoleExtension.WriteError(Resource.ConfiguredButNotConnected);

                        }
                        else if (EduRoamNetwork.IsNetworkInRange(eapConfig))
                        {
                            ConsoleExtension.WriteError(Resource.ConfiguredButUnableToConnect);
                        }
                        else
                        {
                            // Hs2 is not enumerable
                            ConsoleExtension.WriteError(Resource.ConfiguredButProbablyOutOfCoverage);
                        }
                    }

                }
                catch (EduroamAppUserException ex)
                {
                    // TODO, NICE TO HAVE: log the error
                    ConsoleExtension.WriteError(Resource.ErrorNoConnection, ex.UserFacingMessage);
                }

                catch (Exception exc) when (exc is ArgumentException || exc is UnknownInstituteException || exc is UnknownProfileException)
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
