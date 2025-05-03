using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Nrk.FluentCore.Environment;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class LaunchViewModel : SettingsPageVM, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly GameService _gameService;
    private readonly INotificationService _notificationService;

    #region Settings

    public ObservableCollection<string> MinecraftFolders => _settingsService.MinecraftFolders;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMinecraftFoldersEmpty))]
    [BindToSetting(Path = nameof(SettingsService.ActiveMinecraftFolder))]
    public partial string? ActiveMinecraftFolder { get; set; }

    public ObservableCollection<string> Javas => _settingsService.Javas;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsJavasEmpty))]
    [BindToSetting(Path = nameof(SettingsService.ActiveJava))]
    public partial string? ActiveJava { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.JavaMemory))]
    public partial int JavaMemory { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableAutoMemory))]
    public partial bool EnableAutoMemory { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableAutoJava))]
    public partial bool EnableAutoJava { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.GameWindowTitle))]
    public partial string GameWindowTitle { get; set; } = string.Empty;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.GameWindowWidth))]
    public partial int GameWindowWidth { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.GameWindowHeight))]
    public partial int GameWindowHeight { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.GameServerAddress))]
    public partial string GameServerAddress { get; set; } = string.Empty;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableFullScreen))]
    public partial bool EnableFullScreen { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableIndependencyCore))]
    public partial bool EnableIndependencyCore { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.MaxQuickLaunchLatestItem))]
    public partial int MaxQuickLaunchLatestItem { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.GameFilePreferredVerificationMethod))]
    public partial int GameFilePreferredVerificationMethod { get; set; }

    #endregion

    public bool IsMinecraftFoldersEmpty => MinecraftFolders.Count == 0;

    public bool IsJavasEmpty => Javas.Count == 0;

    public LaunchViewModel(
        SettingsService settingsService,
        GameService gameService,
        INotificationService notificationService)
    {
        _settingsService = settingsService;
        _gameService = gameService;
        _notificationService = notificationService;

        (this as ISettingsViewModel).InitializeSettings();
    }

    partial void OnActiveMinecraftFolderChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
            _gameService.ActivateMinecraftFolder(value);
    }

    #region Minecraft Folder

    [RelayCommand]
    async Task BrowserFolder()
    {
        var folderPicker = new FolderPicker();

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        folderPicker.FileTypeFilter.Add("*");
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            if (MinecraftFolders.Contains(folder.Path))
            {
                _notificationService.FolderExisted(folder.Path);

                return;
            }

            _gameService.AddMinecraftFolder(folder.Path);
            OnPropertyChanged(nameof(IsMinecraftFoldersEmpty));
            _notificationService.FolderAdded(folder.Path);
        }
    }

    [RelayCommand]
    void RemoveFolder(string folder)
    {
        _gameService.RemoveMinecraftFolder(folder);
        OnPropertyChanged(nameof(IsMinecraftFoldersEmpty));
    }

    [RelayCommand]
    void ActivateFolder(string folder)
    {
        if (!Directory.Exists(folder))
            return;

        _gameService.ActivateMinecraftFolder(folder);
    }

    [RelayCommand]
    void NavigateFolder(string folder)
    {
        if (!Directory.Exists(folder))
            return;

        _ = Launcher.LaunchFolderPathAsync(folder);
    }

    #endregion

    #region Java

    [RelayCommand]
    void BrowseJava()
    {
        var openFileDialog = new OpenFileDialog
        {
            Multiselect = false,
            Filter = "Javaw Executable File|javaw.exe|Java Executable File|java.exe"
        };

        if (openFileDialog.ShowDialog().GetValueOrDefault(false))
        {
            if (Javas.Contains(openFileDialog.FileName))
            {
                _notificationService.JavaExisted(openFileDialog.FileName);
                return;
            }

            Javas.Add(openFileDialog.FileName);
            ActiveJava = openFileDialog.FileName;

            OnPropertyChanged(nameof(IsJavasEmpty));
            _notificationService.JavaAdded(openFileDialog.FileName);
        }
    }

    [RelayCommand]
    void SearchJava()
    {
        try
        {
            foreach (var java in JavaUtils.SearchJava())
                if (!Javas.Contains(java))
                    Javas.Add(java);

            ActiveJava = Javas.Any() ? Javas[0] : null;

            OnPropertyChanged(nameof(Javas));
            _notificationService.JavaSearched();
        }
        catch (Exception ex) 
        {
            _notificationService.JavaSearchFailed(ex);
        }
    }

    [RelayCommand]
    void RemoveJava(string java)
    {
        Javas.Remove(java);

        if (java == ActiveJava)
            ActiveJava = Javas.Any() ? Javas[0] : null;

        OnPropertyChanged(nameof(IsJavasEmpty));
    }

    [RelayCommand]
    void ActivateJava(string java)
    {
        if (!File.Exists(java))
            return;

        ActiveJava = java;
    }

    [RelayCommand]
    void NavigateJava(string java)
    {
        if (!File.Exists(java))
            return;

        using var process = Process.Start(new ProcessStartInfo("explorer.exe", $"/select,{java}"));
    }

    #endregion
}

public static partial class LaunchViewModelNotifications
{
    [Notification<InfoBar>(Title = "Notifications__FolderExisted", Message = "{path}")]
    public static partial void FolderExisted(this INotificationService notificationService, string path);

    [Notification<InfoBar>(Title = "Notifications__FolderAdded", Message = "{path}", Type = NotificationType.Success)]
    public static partial void FolderAdded(this INotificationService notificationService, string path);

    [Notification<InfoBar>(Title = "Notifications__JavaExisted", Message = "{path}")]
    public static partial void JavaExisted(this INotificationService notificationService, string path);

    [Notification<InfoBar>(Title = "Notifications__JavaAdded", Message = "{path}", Type = NotificationType.Success)]
    public static partial void JavaAdded(this INotificationService notificationService, string path);

    [Notification<InfoBar>(Title = "Notifications__JavaSearched", Type = NotificationType.Success)]
    public static partial void JavaSearched(this INotificationService notificationService);

    [ExceptionNotification(Title = "Notifications__JavaSearchFailed")]
    public static partial void JavaSearchFailed(this INotificationService notificationService, Exception exception);
}