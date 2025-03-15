using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
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
    private readonly NotificationService _notificationService;

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
        NotificationService notificationService)
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
                _notificationService.NotifyMessage(
                    LocalizedStrings.Notifications__AddFolderExistedT,
                    LocalizedStrings.Notifications__AddFolderExistedD,
                    icon: "\uF89A");

                return;
            }

            _gameService.AddMinecraftFolder(folder.Path);
            OnPropertyChanged(nameof(IsMinecraftFoldersEmpty));
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
                _notificationService.NotifyMessage(
                    LocalizedStrings.Notifications__AddJavaExistedT,
                     LocalizedStrings.Notifications__AddJavaExistedD,
                    icon: "\uF89A");

                return;
            }

            Javas.Add(openFileDialog.FileName);
            ActiveJava = openFileDialog.FileName;

            OnPropertyChanged(nameof(IsJavasEmpty));
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

            _notificationService.NotifyWithoutContent(
                LocalizedStrings.Notifications__AddSearchedJavaT,
                icon: "\uE73E");
        }
        catch (Exception ex) 
        {
            _notificationService.NotifyException(LocalizedStrings.Notifications__AddSearchedJavaFailedT, ex);
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