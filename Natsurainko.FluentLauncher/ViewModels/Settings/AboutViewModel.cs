﻿using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Utils.Extensions;
using System;
using System.Threading.Tasks;
using Windows.System;

#if FLUENT_LAUNCHER_PREVIEW_CHANNEL

using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Dialogs;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using System.Text.Json.Nodes;

#endif

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class AboutViewModel : PageVM
{
#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
    private readonly UpdateService _updateService;
    private readonly NotificationService _notificationService;
    private readonly IDialogActivationService<ContentDialogResult> _dialogs;

    public AboutViewModel(UpdateService updateService, IDialogActivationService<ContentDialogResult> dialogs, NotificationService notificationService)
    {
        _updateService = updateService;        
        _notificationService = notificationService;
        _dialogs = dialogs;
    }
#endif

    public string Version => App.Version.GetVersionString();

    public string AppChannel => App.AppChannel;

#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
    [RelayCommand]
    public async Task CheckUpdate()
    {
        var (hasUpdate, releaseJsonResult) = await _updateService.CheckUpdateRelease();

        if (hasUpdate && releaseJsonResult is JsonNode releaseJson)
            await _dialogs.ShowAsync("UpdateDialog", releaseJson);
        else _notificationService.NotifyWithoutContent("this is the latest version", icon: "\uECC5");
    }
#else
    [RelayCommand]
    async Task CheckUpdate() => await Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?productid=9P4NQQXQ942P"));
#endif

    [RelayCommand]
    async Task OpenGit() => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Fluent-Launcher"));

    [RelayCommand]
    async Task OpenAuthor() => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/graphs/contributors"));

    [RelayCommand]
    async Task OpenLicense() => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/blob/main/LICENSE"));
}