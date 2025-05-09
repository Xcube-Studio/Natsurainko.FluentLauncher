using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Utils.Extensions;
using System;
using System.Threading.Tasks;
using Windows.System;

#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Network;
#endif

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class AboutViewModel : PageVM
{
    public string Version => App.Version.GetVersionString();

    public string AppChannel => App.AppChannel;

    [RelayCommand]
    async Task OpenGit() => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Fluent-Launcher"));

    [RelayCommand]
    async Task OpenAuthor() => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/graphs/contributors"));

    [RelayCommand]
    async Task OpenLicense() => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/blob/main/LICENSE"));

#if !FLUENT_LAUNCHER_PREVIEW_CHANNEL
    [RelayCommand]
    async Task CheckUpdate() => await Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?productid=9P4NQQXQ942P"));
#endif
}

#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
internal partial class AboutViewModel(
    UpdateService updateService,
    IDialogActivationService<ContentDialogResult> dialogs,
    INotificationService notificationService) : PageVM
{

    [RelayCommand]
    async Task CheckUpdate()
    {
        var (hasUpdate, release) = await updateService.CheckLauncherUpdateInformation();

        if (hasUpdate)
            await dialogs.ShowAsync("UpdateDialog", release!);
        else notificationService.IsLatestVersion();
    }
}

internal static partial class AboutViewModelNotifications
{
    [Notification<InfoBar>(Title = "Notifications__LauncherIsLatest", Type = NotificationType.Success)]
    public static partial void IsLatestVersion(this INotificationService notificationService);
}
#endif