using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Common;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels;

internal partial class ShellViewModel : PageVM, INavigationAware, IRecipient<GlobalNavigationMessage>
{
    private readonly LaunchService _launchService;
    private readonly DownloadService _downloadService;

    public bool _onNavigatedTo = false;

    public INavigationService NavigationService { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LaunchTasksInfoBadgeOpacity))]
    public partial int RunningLaunchTasks { get; set; }

    public float LaunchTasksInfoBadgeOpacity => RunningLaunchTasks == 0 ? 0 : 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadTasksInfoBadgeOpacity))]
    public partial int RunningDownloadTasks { get; set; }

    public float DownloadTasksInfoBadgeOpacity => RunningDownloadTasks == 0 ? 0 : 1;

    public ShellViewModel(
        LaunchService launchService, 
        DownloadService downloadService, 
        INavigationService shellNavigationService)
    {
        _launchService = launchService;
        _downloadService = downloadService;
        NavigationService = shellNavigationService;

        _launchService.TaskListStateChanged += (_, e) =>
            Dispatcher.TryEnqueue(() =>
                RunningLaunchTasks = _launchService.LaunchTasks
                    .Where(x => x.TaskState == TaskState.Running || x.TaskState == TaskState.Prepared)
                    .Count());

        _downloadService.TaskListStateChanged += (_, e) =>
            Dispatcher.TryEnqueue(() =>
                RunningDownloadTasks = _downloadService.DownloadTasks
                    .Where(x => x.TaskState == TaskState.Running || x.TaskState == TaskState.Prepared)
                    .Count());
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is string pageKey)
        {
            NavigationService.NavigateTo(pageKey);
            _onNavigatedTo = true;
        }
        else NavigationService.NavigateTo("HomePage");
    }

    void IRecipient<GlobalNavigationMessage>.Receive(GlobalNavigationMessage message)
        => Dispatcher.TryEnqueue(() => this.NavigationService.NavigateTo(message.Value, message.Parameter));
}
