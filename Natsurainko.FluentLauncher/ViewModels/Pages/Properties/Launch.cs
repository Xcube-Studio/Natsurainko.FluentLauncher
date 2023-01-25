using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentLauncher.Components.FluentCore;
using Natsurainko.FluentLauncher.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCore = Natsurainko.FluentLauncher.Components.FluentCore.GameCore;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Properties;

public partial class Launch : ObservableObject
{
    public Launch(GameCore core) 
    {
        CoreProfile = core.CoreProfile;

        enableSpecialSetting = CoreProfile.EnableSpecialSetting;
        width = (CoreProfile.LaunchSetting?.GameWindowSetting?.Width).GetValueOrDefault(854);
        height = (CoreProfile.LaunchSetting?.GameWindowSetting?.Height).GetValueOrDefault(480);
        enableFullScreen = (CoreProfile.LaunchSetting?.GameWindowSetting?.IsFullscreen).GetValueOrDefault(false);
        title = CoreProfile.LaunchSetting?.GameWindowSetting?.WindowTitle;
        profilePath = CoreProfile.FilePath;
        serverAddress = (CoreProfile.LaunchSetting != null && CoreProfile.LaunchSetting.ServerSetting != null)
            ? CoreProfile.LaunchSetting.ServerSetting.ToString() 
            : null;

        pathVisibility = File.Exists(CoreProfile.FilePath)
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    public CoreProfile CoreProfile;

    [ObservableProperty]
    private bool enableSpecialSetting;

    [ObservableProperty]
    private int width;

    [ObservableProperty]
    private int height;

    [ObservableProperty]
    private string serverAddress;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private bool enableIndependencyCore;

    [ObservableProperty]
    private bool enableFullScreen;

    [ObservableProperty]
    private string profilePath;

    [ObservableProperty]
    private Visibility pathVisibility;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        CoreProfile.EnableSpecialSetting= enableSpecialSetting;
        CoreProfile.LaunchSetting = new LaunchSetting
        {
            EnableIndependencyCore = enableIndependencyCore,
            GameWindowSetting = new GameWindowSetting
            {
                Height = height,
                Width = width,
                IsFullscreen = enableFullScreen,
                WindowTitle = title,
            },
            ServerSetting = !string.IsNullOrEmpty(serverAddress)
                ? new ServerSetting(serverAddress)
                : null
        };

        if (e.PropertyName != nameof(PathVisibility))
            PathVisibility = File.Exists(CoreProfile.FilePath)
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    [RelayCommand]
    public void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo("explorer.exe", $"/select,{ProfilePath}"));
    }
}
