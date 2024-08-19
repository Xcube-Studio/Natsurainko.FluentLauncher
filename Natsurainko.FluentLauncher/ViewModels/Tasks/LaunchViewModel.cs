using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

internal partial class LaunchViewModel : ObservableObject, INavigationAware
{
    private readonly LaunchSessions _launchSessions;
    private readonly INavigationService _navigationService;

    public ObservableCollection<object> Tasks { get; } = [];

    public LaunchViewModel(LaunchSessions launchSessions, INavigationService navigationService)
    {
        _launchSessions = launchSessions;
        _navigationService = navigationService;

        launchSessions.SessionViewModels.CollectionChanged += SessionViewModels_CollectionChanged;

        foreach (var item in launchSessions.SessionViewModels)
            Tasks.Add(item);
    }

    private void SessionViewModels_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        foreach (var item in e.NewItems!)
            App.DispatcherQueue.TryEnqueue(() => Tasks.Insert(0, item));
    }

    [RelayCommand]
    void Unloaded()
    {
        _launchSessions.SessionViewModels.CollectionChanged -= SessionViewModels_CollectionChanged;
    }

    [RelayCommand]
    void GoToHome() => _navigationService.NavigateTo("HomePage");
}
