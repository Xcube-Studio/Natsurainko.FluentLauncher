using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Components.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

internal partial class LaunchViewModel : ObservableObject
{
    public ReadOnlyObservableCollection<LaunchSessionViewModel> LaunchSessions { get; init; }

    private readonly INavigationService _shellNavigationService;
    private readonly SettingsService _settings;
    private readonly ObservableCollection<LaunchSessionViewModel> _launchSessions;

    public LaunchViewModel(LaunchSessions launchSessions, INavigationService navigationService, SettingsService settingsService)
    {
        LaunchSessions = new(launchSessions.SessionViewModels);
        _launchSessions = launchSessions.SessionViewModels;
        _shellNavigationService = navigationService.Parent ?? throw new InvalidOperationException("Cannot obtain the shell navigaiton service");
        _settings = settingsService;

        launchSessions.SessionViewModels.CollectionChanged += LaunchSessions_CollectionChanged;
    }

    ~LaunchViewModel()
    {
        _launchSessions.CollectionChanged -= LaunchSessions_CollectionChanged;
    }

    private void LaunchSessions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TipVisibility));
    }

    public Visibility TipVisibility => LaunchSessions.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    [RelayCommand]
    public void Home() => _shellNavigationService.NavigateTo("HomePage");
}
