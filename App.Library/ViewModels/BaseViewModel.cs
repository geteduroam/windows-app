// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseViewModel.cs" company="Winvision bv">
//   Copyright (c) Winvision bv. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Threading.Tasks;

using App.Library.Command;
using App.Library.Language;

namespace App.Library.ViewModels;

public abstract class BaseViewModel : NotifyPropertyChanged
{
    public ILanguageText LanguageText { get; }

    protected MainViewModel MainViewModel { get; }

    protected BaseViewModel(MainViewModel mainViewModel)
    {
        this.MainViewModel = mainViewModel;
        this.LanguageText = mainViewModel.LanguageText;
        this.NextCommand = new AsyncCommand(this.GoNextAsync, this.CanGoNext);
    }

    public AsyncCommand NextCommand { get; protected set; }

    protected abstract bool CanGoNext();

    protected abstract Task GoNextAsync();

    public bool IsLoading { get; protected set; }

    protected void SetIsLoading(bool value)
    {
        this.IsLoading = value;
        this.CallPropertyChanged(nameof(this.IsLoading));
    }

    private async Task RunExecuteActionAsync()
    {
        this.SetIsLoading(true);
        try
        {
            await GoNextAsync();
        }
        finally
        {
            this.SetIsLoading(false);
        }
    }
}