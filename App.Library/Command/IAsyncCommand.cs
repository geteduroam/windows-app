using System.Threading.Tasks;
using System.Windows.Input;

namespace App.Library.Command
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();

        bool CanExecute();
    }
}