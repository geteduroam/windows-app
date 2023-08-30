using System;
using System.Diagnostics;
using System.Windows.Input;

namespace App.Library.Command
{
    public class DelegateCommand : ICommand
    {
        public DelegateCommand(
                Action commandAction,
                Func<bool>? canExecute = null)
        {
            this.CommandAction = commandAction;
            this.CanExecuteFunc = canExecute;
        }

        public DelegateCommand(Action<object?> commandAction)
        {
            this.CommandWithParamAction = commandAction;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public Action? CommandAction { get; set; }

        public Func<bool>? CanExecuteFunc { get; set; }

        public Action<object?>? CommandWithParamAction { get; set; }

        public void Execute(object? parameter)
        {
            if (this.CommandWithParamAction != null)
            {
                Debug.WriteLine($"parameter: {parameter}");
                this.CommandWithParamAction(parameter);
            }
            else if (this.CommandAction != null)
            {
                this.CommandAction();
            }
            else
            {
                throw new InvalidOperationException("No Command action implemented");
            }
        }

        public bool CanExecute(object? parameter)
        {
            return this.CanExecuteFunc == null || this.CanExecuteFunc();
        }

        public static void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}