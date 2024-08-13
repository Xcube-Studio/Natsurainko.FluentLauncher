using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Network;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

internal partial class DefaultViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly LaunchSessions _launchSessions;
    private readonly DownloadService _downloadService;

    public ObservableCollection<object> Tasks { get; } = [];

    public DefaultViewModel(
        LaunchSessions launchSessions, 
        DownloadService downloadService, 
        INavigationService navigationService)
    {
        _navigationService = navigationService;
        _launchSessions = launchSessions;
        _downloadService = downloadService;

        launchSessions.SessionViewModels.CollectionChanged += SessionViewModels_CollectionChanged;

        foreach (var item in launchSessions.SessionViewModels)
            Tasks.Add(item);

        foreach (var item in downloadService.DownloadProcesses)
            Tasks.Add(item);
    }

    private void SessionViewModels_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        App.DispatcherQueue.TryEnqueue(() =>
        {
            foreach (var item in e.NewItems!)
                Tasks.Insert(0, item);
        });
    }

    [RelayCommand]
    void Unloaded()
    {
        _launchSessions.SessionViewModels.CollectionChanged -= SessionViewModels_CollectionChanged;
    }
}
