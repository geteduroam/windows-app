// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseViewModel.cs" company="Winvision bv">
//   Copyright (c) Winvision bv. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using App.Library.Language;

namespace App.Library.ViewModels;

public abstract class BaseViewModel : NotifyPropertyChanged
{
    public ILanguageText LanguageText { get; }

    protected BaseViewModel(ILanguageText languageText)
    {
        this.LanguageText = languageText;
    }
}