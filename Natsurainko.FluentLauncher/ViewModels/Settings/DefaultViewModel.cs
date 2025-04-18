﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class DefaultViewModel : SettingsPageVM, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly INavigationService _navigationService;

    public DefaultViewModel(SettingsService settingsService, INavigationService navigationService)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;

        (this as ISettingsViewModel).InitializeSettings();
    }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CurrentLanguage))]
    public partial string CurrentLanguage { get; set; } = null!;

    public string Version => App.Version.GetVersionString();

    public string AppChannel => App.AppChannel;

    partial void OnCurrentLanguageChanged(string oldValue, string newValue)
    {
        if (oldValue is not null) // oldValue is null at startup
            LocalizedStrings.ApplyLanguage(CurrentLanguage);
    }

    [RelayCommand]
    void CardClick(string tag) => _navigationService.NavigateTo(tag);
}
