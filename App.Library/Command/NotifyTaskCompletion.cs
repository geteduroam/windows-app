using System;
using System.ComponentModel;
using System.Threading.Tasks;

/// <summary>
/// https://learn.microsoft.com/en-us/archive/msdn-magazine/2014/march/async-programming-patterns-for-asynchronous-mvvm-applications-data-binding
/// </summary>
/// <typeparam name="TResult"></typeparam>
public sealed class NotifyTaskCompletion<TResult> : INotifyPropertyChanged
{
    public NotifyTaskCompletion(Task<TResult> task)
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