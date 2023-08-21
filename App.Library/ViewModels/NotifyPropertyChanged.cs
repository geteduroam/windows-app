using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace App.Library.ViewModels
{
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary>
        /// Called when a property changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Calls the PropertyChanged event
        /// </summary>
        public virtual void CallPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var handler = this.PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}