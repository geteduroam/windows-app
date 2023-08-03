using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace App.Library.Binding
{
    /// <summary>
    ///  Create a async property to be used in a ViewModel/xaml. With async is meant, the value is retrieved async (and is therefore awaited)
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <example>
    /// ViewModel:
    /// <code>
    ///     public class SelectInstitutionViewModel : BaseViewModel
    ///     {
    ///     	private string searchText;
    ///     
    ///     	public SelectInstitutionViewModel(MainViewModel owner)
    ///     		: base(owner)
    ///     	{
    ///     		this.searchText = string.Empty;
    ///     		this.Institutions = new AsyncProperty<ObservableCollection<IdentityProvider>>(this.GetInstitutionsAsync());
    ///     	}
    ///     
    ///     	public AsyncProperty<ObservableCollection<IdentityProvider>> Institutions
    ///     	{
    ///     		get; private set;
    ///     	}
    ///     
    ///     	public async Task<ObservableCollection<IdentityProvider>> GetInstitutionsAsync()
    ///     	{
    ///     		var institutesTask = new GetInstitutesTask();
    ///     
    ///     		var institutes = await institutesTask.GetAsync();
    ///     		return new ObservableCollection<IdentityProvider>(institutes);
    ///     	}
    ///     }
    /// </ccode>
    /// Binding in .xaml:
    /// <code>
    ///     ListBox
    /// 	    x:Name="lbInstitutions"
    /// 	    ItemsSource="{Binding Institutions.Result}"/>
    /// </code>
    /// </example>
    /// <ex
    /// <remarks>
    ///     Class based on https://learn.microsoft.com/en-us/archive/msdn-magazine/2014/march/async-programming-patterns-for-asynchronous-mvvm-applications-data-binding 
    ///      (Stephen Cleary, March 2014)
    /// </remarks>
    public sealed class AsyncProperty<TResult> : INotifyPropertyChanged
    {
        public AsyncProperty(Task<TResult> task)
        {
            this.Task = task;
            if (!task.IsCompleted)
            {
                var _ = this.WatchTaskAsync(task);
            }
        }

        private async Task WatchTaskAsync(Task task)
        {
            try
            {
                await task;
            }
            catch
            {
            }
            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
            {
                return;
            }

            propertyChanged(this, new PropertyChangedEventArgs(nameof(this.Status)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(this.IsCompleted)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(this.IsNotCompleted)));

            if (task.IsCanceled)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.IsCanceled)));
            }
            else if (task.IsFaulted)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.IsFaulted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.Exception)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.InnerException)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.ErrorMessage)));
            }
            else
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.IsSuccessfullyCompleted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(this.Result)));
            }
        }
        public Task<TResult> Task { get; private set; }

        public TResult? Result
        {
            get
            {
                return (this.Task.Status == TaskStatus.RanToCompletion) ? this.Task.Result : default;
            }
        }

        public TaskStatus Status { get { return this.Task.Status; } }
        public bool IsCompleted { get { return this.Task.IsCompleted; } }
        public bool IsNotCompleted { get { return !this.Task.IsCompleted; } }
        public bool IsSuccessfullyCompleted
        {
            get
            {
                return this.Task.Status == TaskStatus.RanToCompletion;
            }
        }
        public bool IsCanceled { get { return this.Task.IsCanceled; } }
        public bool IsFaulted { get { return this.Task.IsFaulted; } }

        public AggregateException? Exception { get { return this.Task.Exception; } }
        public Exception? InnerException
        {
            get
            {
                return this.Exception?.InnerException;
            }
        }

        public string? ErrorMessage
        {
            get
            {
                return this.InnerException?.Message;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}