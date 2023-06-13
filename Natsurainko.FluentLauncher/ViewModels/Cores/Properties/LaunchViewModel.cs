using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentLauncher.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using GameCore = Natsurainko.FluentLauncher.Components.FluentCore.GameCore;

namespace Natsurainko.FluentLauncher.ViewModels.Cores.Properties;

partial class LaunchViewModel : ObservableObject
{
    private bool isLoading = true;

    public LaunchViewModel(GameCore core)
    {
        CoreProfile = core.CoreProfile;

        EnableSpecialSetting = CoreProfile.EnableSpecialSetting;
        EnableFullScreen = (CoreProfile.LaunchSetting?.GameWindowSetting?.IsFullscreen).GetValueOrDefault(false);
        EnableIndependencyCore = (CoreProfile.LaunchSetting?.EnableIndependencyCore).GetValueOrDefault(false);

        Width = (CoreProfile.LaunchSetting?.GameWindowSetting?.Width).GetValueOrDefault(854);
        Height = (CoreProfile.LaunchSetting?.GameWindowSetting?.Height).GetValueOrDefault(480);
        Title = CoreProfile.LaunchSetting?.GameWindowSetting?.WindowTitle;
        ProfilePath = CoreProfile.FilePath;
        ServerAddress = (CoreProfile.LaunchSetting != null && CoreProfile.LaunchSetting.ServerSetting != null)
            ? CoreProfile.LaunchSetting.ServerSetting.ToString()
            : null;

        PathVisibility = File.Exists(CoreProfile.FilePath)
                ? Visibility.Visible
                : Visibility.Collapsed;

        JvmParameters = CoreProfile.JvmSetting?.JvmParameters;

        isLoading = false;
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

    [ObservableProperty]
    private string jvmParameters;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName != nameof(PathVisibility))
            PathVisibility = File.Exists(CoreProfile.FilePath)
                ? Visibility.Visible
                : Visibility.Collapsed;

        if (isLoading)
            return;

        CoreProfile.EnableSpecialSetting = enableSpecialSetting;
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
        CoreProfile.JvmSetting = new Models.JvmSetting
        {
            JvmParameters = jvmParameters,
        };
    }

    [RelayCommand]
    public void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo("explorer.exe", $"/select,{ProfilePath}"));
    }
}
