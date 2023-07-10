using System;
using System.Windows.Input;

namespace App.Library.Command
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public Action CommandAction { get; set; }

        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            this.CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return this.CanExecuteFunc == null || this.CanExecuteFunc();
        }
    }
}