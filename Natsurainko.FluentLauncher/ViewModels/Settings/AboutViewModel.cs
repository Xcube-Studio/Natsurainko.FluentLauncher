﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Dialogs;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class AboutViewModel : ObservableObject
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

#if DEBUG
    [ObservableProperty]
    public partial string Edition { get; set; } = LocalizedStrings.Settings_AboutPage__Debug;
#else
    [ObservableProperty]
    public partial string Edition { get; set; } = LocalizedStrings.Settings_AboutPage__Release;
#endif

    public string Channel => App.AppChannel.ToUpper();

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
    public async Task CheckUpdate()
        => await Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?productid=9P4NQQXQ942P"));
#endif

    [RelayCommand]
    public async Task OpenGit()
        => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Fluent-Launcher"));

    [RelayCommand]
    public async Task OpenAuthor()
        => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/graphs/contributors"));

    [RelayCommand]
    public async Task OpenLicense()
        => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/blob/main/LICENSE"));

}