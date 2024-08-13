using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

internal partial class LaunchViewModel : ObservableObject, INavigationAware
{
    private readonly LaunchSessions _launchSessions;

    public ObservableCollection<object> Tasks { get; } = [];

    public LaunchViewModel(LaunchSessions launchSessions)
    {
        _launchSessions = launchSessions;

        launchSessions.SessionViewModels.CollectionChanged += SessionViewModels_CollectionChanged;

        foreach (var item in launchSessions.SessionViewModels)
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
