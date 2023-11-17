using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Components.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

internal partial class LaunchViewModel : ObservableObject
{
    public ReadOnlyObservableCollection<LaunchSessionViewModel> LaunchProcesses { get; init; }

    private readonly INavigationService _shellNavigationService;
    private readonly SettingsService _settings;

    public LaunchViewModel(LaunchService launchService, INavigationService navigationService, SettingsService settingsService)
    {
        LaunchProcesses = launchService.Sessions; // TODO: create view models from MinecraftSession list
        _shellNavigationService = navigationService.Parent ?? throw new InvalidOperationException("Cannot obtain the shell navigaiton service");
        _settings = settingsService;

        TipVisibility = LaunchProcesses.Any()
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    [ObservableProperty]
    private Visibility tipVisibility;

    [RelayCommand]
    public void Home() => _shellNavigationService.NavigateTo(_settings.UseNewHomePage ? "NewHomePage" : "HomePage");
}
