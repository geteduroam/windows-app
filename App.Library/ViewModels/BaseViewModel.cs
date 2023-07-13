// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseViewModel.cs" company="Winvision bv">
//   Copyright (c) Winvision bv. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using App.Library.Command;
using App.Library.Language;

namespace App.Library.ViewModels;

public abstract class BaseViewModel : NotifyPropertyChanged
{
    public ILanguageText LanguageText { get; }

    public MainViewModel Owner { get; }

    protected BaseViewModel(MainViewModel owner)
    {
        this.Owner = owner;
        this.LanguageText = owner.LanguageText;
        this.NextCommand = new AsyncCommand(this.ExecuteNavigateNextActionAsync, this.CanNavigateNextAsync);
        this.PreviousCommand = new AsyncCommand(this.ExecuteNavigatePreviousActionAsync, this.CanNavigatePrevious);
    }

    public AsyncCommand NextCommand { get; protected set; }

    public AsyncCommand PreviousCommand { get; protected set; }

    protected abstract bool CanNavigateNextAsync();

    protected abstract Task NavigateNextAsync();

    protected abstract bool CanNavigatePrevious();

    protected abstract Task NavigatePreviousAsync();

    public bool IsLoading { get; protected set; }

    protected void SetIsLoading(bool value)
    {
        this.IsLoading = value;
        this.CallPropertyChanged(nameof(this.IsLoading));
    }

    private async Task ExecuteNavigateNextActionAsync()
    {
        this.SetIsLoading(true);
        try
        {
            await this.NavigateNextAsync();
        }
        finally
        {
            this.SetIsLoading(false);
        }
    }

    private async Task ExecuteNavigatePreviousActionAsync()
    {
        this.SetIsLoading(true);
        try
        {
            await this.NavigatePreviousAsync();
        }
        finally
        {
            this.SetIsLoading(false);
        }
    }
}