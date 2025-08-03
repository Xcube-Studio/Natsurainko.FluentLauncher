using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI.Messaging;

namespace Natsurainko.FluentLauncher.ViewModels;

internal partial class ShellViewModel : PageVM, INavigationAware, 
    IRecipient<GlobalNavigationMessage>,
    IRecipient<BackgroundTaskCountChangedMessage>
{
    private readonly LaunchService _launchService;
    private readonly DownloadService _downloadService;

    public INavigationService NavigationService { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LaunchTasksInfoBadgeOpacity))]
    public partial int RunningLaunchTasks { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadTasksInfoBadgeOpacity))]
    public partial int RunningDownloadTasks { get; set; }

    public float LaunchTasksInfoBadgeOpacity => RunningLaunchTasks == 0 ? 0 : 1;

    public float DownloadTasksInfoBadgeOpacity => RunningDownloadTasks == 0 ? 0 : 1;

    public ShellViewModel(
        LaunchService launchService, 
        DownloadService downloadService, 
        INavigationService shellNavigationService)
    {
        _launchService = launchService;
        _downloadService = downloadService;
        NavigationService = shellNavigationService;

        RunningLaunchTasks = _launchService.RunningTasks;
        RunningDownloadTasks = _downloadService.RunningTasks;
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is string pageKey)
            NavigationService.NavigateTo(pageKey);
        else NavigationService.NavigateTo("HomePage");
    }

    async void IRecipient<GlobalNavigationMessage>.Receive(GlobalNavigationMessage message)
        => await Dispatcher.EnqueueAsync(() => this.NavigationService.NavigateTo(message.Value, message.Parameter));

    async void IRecipient<BackgroundTaskCountChangedMessage>.Receive(BackgroundTaskCountChangedMessage _)
        => await Dispatcher.EnqueueAsync(() => (RunningLaunchTasks, RunningDownloadTasks) = (_launchService.RunningTasks, _downloadService.RunningTasks));
}
