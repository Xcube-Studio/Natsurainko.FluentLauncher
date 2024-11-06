using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.ViewModels.Common;
using System.Collections.ObjectModel;
using System.Linq;


namespace Natsurainko.FluentLauncher.ViewModels;

internal partial class ShellViewModel : ObservableObject, INavigationAware
{
    private readonly LaunchService _launchService;
    private readonly DownloadService _downloadService;
    private readonly INavigationService _shellNavigationService;

    public bool _onNavigatedTo = false;

    public INavigationService NavigationService => _shellNavigationService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LaunchTasksInfoBadgeOpacity))]
    private int runningLaunchTasks;

    public float LaunchTasksInfoBadgeOpacity => RunningLaunchTasks == 0 ? 0 : 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadTasksInfoBadgeOpacity))]
    private int runningDownloadTasks;

    public float DownloadTasksInfoBadgeOpacity => RunningDownloadTasks == 0 ? 0 : 1;

    public ShellViewModel(
        LaunchService launchService, 
        DownloadService downloadService, 
        INavigationService shellNavigationService)
    {
        _launchService = launchService;
        _downloadService = downloadService;
        _shellNavigationService = shellNavigationService;

        _launchService.TaskListStateChanged += (_, e) =>
            App.DispatcherQueue.TryEnqueue(() =>
                RunningLaunchTasks = _launchService.LaunchTasks
                    .Where(x => x.TaskState == TaskState.Running || x.TaskState == TaskState.Prepared)
                    .Count());

        _downloadService.TaskListStateChanged += (_, e) =>
            App.DispatcherQueue.TryEnqueue(() =>
                RunningDownloadTasks = _downloadService.DownloadTasks
                    .Where(x => x.TaskState == TaskState.Running || x.TaskState == TaskState.Prepared)
                    .Count());
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is string pageKey)
        {
            _shellNavigationService.NavigateTo(pageKey);
            _onNavigatedTo = true;
        }
        else _shellNavigationService.NavigateTo("HomePage");
    }
}
