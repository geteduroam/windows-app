using EduRoam.Connect;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks;

using System.CommandLine;
using System.Security;

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
                    var connected = false;
                    IList<string> messages = new List<string>();

                    var connectionType = await connectTask.GetConnectionTypeAsync();

                    switch (connectionType)
                    {
                        case ConnectionType.Credentials:
                            (connected, messages) = await this.ConnectWithCredentialsAsync(connectTask);
                            break;
                        case ConnectionType.CertPass:
                            (connected, messages) = await this.ConnectWithCertPassAsync(connectTask);
                            break;
                        case ConnectionType.CertAndCertPass:
                            (connected, messages) = await this.ConnectWithCertAndCertPassAsync(connectTask);
                            break;
                        default:
                            (connected, messages) = await connectTask.ConnectAsync();
                            break;
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

        private Task<(bool connected, IList<string> messages)> ConnectWithCertAndCertPassAsync(ConnectTask connectTask)
        {
            throw new NotImplementedException();
        }

        private Task<(bool connected, IList<string> messages)> ConnectWithCertPassAsync(ConnectTask connectTask)
        {
            throw new NotImplementedException();
        }

        private async Task<(bool connected, IList<string> messages)> ConnectWithCredentialsAsync(ConnectTask connectTask)
        {
            Console.WriteLine(Resource.ConnectionUsernameAndPasswordRequired);
            Console.Write($"{Resource.Username}: ");
            var userName = Console.ReadLine();

            Console.Write($"{Resource.Password}: ");
            using var password = ReadPassword();

            var connected = false;

            var (areValid, messages) = await connectTask.ValidateCredentialsAsync(userName, password);

            if (areValid)
            {
                (connected, var message) = await connectTask.ConnectAsync(userName, password);

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
        private static SecureString ReadPassword()
        {
            var pass = new SecureString();
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass.RemoveAt(pass.Length - 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass.AppendChar(keyInfo.KeyChar);
                }
            } while (keyInfo.Key != ConsoleKey.Enter);

            return pass;
        }
    }
}
