// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseViewModel.cs" company="Winvision bv">
//   Copyright (c) Winvision bv. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using App.Library.Command;

using System.Linq;
using System.Threading.Tasks;

using SharedResources = EduRoam.Localization.Resources;

namespace App.Library.ViewModels;

public abstract class BaseViewModel : NotifyPropertyChanged
{
    public MainViewModel Owner { get; }

    protected BaseViewModel(MainViewModel owner)
    {
        this.Owner = owner;
        this.NextCommand = new AsyncCommand(this.ExecuteNavigateNextActionAsync, this.CanNavigateNextAsync);
        this.PreviousCommand = new AsyncCommand(this.ExecuteNavigatePreviousActionAsync, this.CanNavigatePrevious);
    }

    public AsyncCommand NextCommand { get; protected set; }

    public AsyncCommand PreviousCommand { get; protected set; }

    public abstract string PageTitle { get; }

    public virtual bool ShowNavigatePrevious => true;

    public virtual bool ShowNavigateNext => true;

    public virtual bool ShowLogo => false;

    public virtual string PreviousTitle => SharedResources.ButtonPrevious;

    public virtual string NextTitle => SharedResources.ButtonNext;

    protected abstract bool CanNavigateNextAsync();

    protected abstract Task NavigateNextAsync();

    protected virtual bool CanNavigatePrevious()
    {
        return this.Owner.State.NavigationHistory.Any();
    }

    protected virtual Task NavigatePreviousAsync()
    {
        this.Owner.SetPreviousActiveContent();
        return Task.CompletedTask;
    }

    private bool isLoading = false;
    public bool IsLoading
    { 
        get => this.isLoading;
        protected set
        {
            this.isLoading = value;
            this.CallPropertyChanged(nameof(this.IsLoading));
        }
    }

    private async Task ExecuteNavigateNextActionAsync()
    {
        this.IsLoading = true;
        try
        {
            await this.NavigateNextAsync();
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    private async Task ExecuteNavigatePreviousActionAsync()
    {
        this.IsLoading = true;
        try
        {
            await this.NavigatePreviousAsync();
        }
        finally
        {
            this.IsLoading = false;
        }
    }
}