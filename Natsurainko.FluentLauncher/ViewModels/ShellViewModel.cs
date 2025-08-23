using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Notification;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Views;

namespace Natsurainko.FluentLauncher.ViewModels;

internal partial class ShellViewModel : PageVM<ShellPage>, INavigationAware,
    IRecipient<GlobalNavigationMessage>,
    IRecipient<BackgroundTaskCountChangedMessage>,
    IRecipient<DownloadTaskCreatedMessage>
{
    private readonly LaunchService _launchService;
    private readonly DownloadService _downloadService;
    private readonly INotificationService _notificationService;

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
        INotificationService notificationService,
        INavigationService shellNavigationService)
    {
        _launchService = launchService;
        _downloadService = downloadService;
        _notificationService = notificationService;
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

    async void IRecipient<DownloadTaskCreatedMessage>.Receive(DownloadTaskCreatedMessage message)
    {
        await Dispatcher.EnqueueAsync(() =>
        {
            _notificationService.Show(new TargetNotification
            {
                Title = message.Value switch
                {
                    0 => LocalizedStrings.Notifications__TaskCreated_ResourceDownload,
                    1 => LocalizedStrings.Notifications__TaskCreated_InstallModpack,
                    _ => string.Empty
                },
                Message = LocalizedStrings.Notifications__TaskCreated_Description_Download,
                FontIcon = "\uE945",
                IsClosable = true,
                Target = Page.DownloadTaskNavigationViewItem
            });
        });
    }
}