using EduRoam.Connect;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks;
using EduRoam.Connect.Tasks.Connectors;

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

                var connector = await connectTask.GetConnectorAsync();
                if (connector == null)
                {
                    ConsoleExtension.WriteError(Resource.ErrorEapConfigIsEmpty);
                    return;
                }

                try
                {
                    var connected = false;
                    IList<string> messages = new List<string>();

                    switch (connector)
                    {
                        case CredentialsConnector credentialsConnector:
                            (connected, messages) = await this.ConnectWithCredentialsAsync(credentialsConnector);
                            break;
                        case CertPassConnector certPassConnector:
                            (connected, messages) = await this.ConnectWithCertPassAsync(certPassConnector);
                            break;
                        case CertAndCertPassConnector certAndCertPassConnector:
                            (connected, messages) = await this.ConnectWithCertAndCertPassAsync(certAndCertPassConnector);
                            break;
                        case DefaultConnector defaultConnector:
                            (connected, messages) = await this.ConnectAsync(defaultConnector);
                            break;
                        default:
                            throw new NotSupportedException(string.Format(Resource.ErrorUnsupportedConnectionType, connector.GetType().Name));
                    }

                    if (connected)
                    {
                        ConsoleExtension.WriteStatus(string.Join("\n", messages));
                    }
                    else
                    {
                        ConsoleExtension.WriteError(string.Join("\n", messages));
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

        private Task<(bool connected, IList<string> messages)> ConnectAsync(DefaultConnector connector)
        {
            throw new NotImplementedException();
        }

        private Task<(bool connected, IList<string> messages)> ConnectWithCertAndCertPassAsync(CertAndCertPassConnector connector)
        {
            throw new NotImplementedException();
        }

        private Task<(bool connected, IList<string> messages)> ConnectWithCertPassAsync(CertPassConnector connector)
        {
            throw new NotImplementedException();
        }

        private async Task<(bool connected, IList<string> messages)> ConnectWithCredentialsAsync(CredentialsConnector connector)
        {
            Console.WriteLine(Resource.ConnectionUsernameAndPasswordRequired);
            Console.Write($"{Resource.Username}: ");
            var userName = Console.ReadLine();

            Console.Write($"{Resource.Password}: ");
            var password = ReadPassword();

            connector.Credentials = new ConnectorCredentials(userName, password);

            var (connected, messages) = connector.ValidateCredentials();

            if (connected)
            {
                (connected, var message) = await connector.ConnectAsync(userName!, password);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    messages.Add(message);
                }
            }

            return (connected, messages);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>Based on https://stackoverflow.com/a/3404522</remarks>
        private static string ReadPassword()
        {
            var pass = string.Empty;
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass += pass.Length - 1;
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
