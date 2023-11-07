using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Environment;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class LaunchViewModel : SettingsViewModelBase, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly GameService _gameService;
    private readonly NotificationService _notificationService;
    private readonly INavigationService _navigationService;

    #region Settings

    public ObservableCollection<string> MinecraftFolders => _settingsService.MinecraftFolders;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMinecraftFoldersEmpty))]
    [BindToSetting(Path = nameof(SettingsService.ActiveMinecraftFolder))]
    private string activeMinecraftFolder;

    public ObservableCollection<string> Javas => _settingsService.Javas;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsJavasEmpty))]
    [BindToSetting(Path = nameof(SettingsService.ActiveJava))]
    private string activeJava;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.JavaMemory))]
    private int javaMemory;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableAutoMemory))]
    private bool enableAutoMemory;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableAutoJava))]
    private bool enableAutoJava;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.GameWindowTitle))]
    private string gameWindowTitle;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.GameWindowWidth))]
    private int gameWindowWidth;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.GameWindowHeight))]
    private int gameWindowHeight;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.GameServerAddress))]
    private string gameServerAddress;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableFullScreen))]
    private bool enableFullScreen;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableIndependencyCore))]
    private bool enableIndependencyCore;

    #endregion

    public bool IsMinecraftFoldersEmpty => MinecraftFolders.Count == 0;

    public bool IsJavasEmpty => Javas.Count == 0;

    public LaunchViewModel(
        SettingsService settingsService,
        GameService gameService,
        NotificationService notificationService,
        INavigationService navigationService)
    {
        _settingsService = settingsService;
        _gameService = gameService;
        _notificationService = notificationService;
        _navigationService = navigationService;

        (this as ISettingsViewModel).InitializeSettings();
        _navigationService = navigationService;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(ActiveMinecraftFolder))
        {
            if (!string.IsNullOrEmpty(ActiveMinecraftFolder))
                _gameService.ActivateMinecraftFolder(ActiveMinecraftFolder);
        }
    }

    [RelayCommand]
    public Task BrowserFolder() => Task.Run(async () =>
    {
        var folderPicker = new FolderPicker();

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        folderPicker.FileTypeFilter.Add("*");
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
            App.DispatcherQueue.TryEnqueue(() =>
            {
                if (MinecraftFolders.Contains(folder.Path))
                {
                    _notificationService.NotifyMessage(
                        ResourceUtils.GetValue("Notifications", "_AddFolderExistedT"),
                        ResourceUtils.GetValue("Notifications", "_AddFolderExistedD"),
                        icon: "\uF89A");

                    return;
                }

                _gameService.AddMinecraftFolder(folder.Path);
                OnPropertyChanged(nameof(IsMinecraftFoldersEmpty));
            });

    });

    [RelayCommand]
    public void BrowserJava()
    {
        var openFileDialog = new OpenFileDialog();
        openFileDialog.Multiselect = false;
        openFileDialog.Filter = "Javaw Executable File|javaw.exe|Java Executable File|java.exe";

        if (openFileDialog.ShowDialog().GetValueOrDefault(false))
            App.DispatcherQueue.TryEnqueue(() =>
            {
                Javas.Add(openFileDialog.FileName);
                ActiveJava = openFileDialog.FileName;

                OnPropertyChanged(nameof(IsJavasEmpty));
            });
    }

    [RelayCommand]
    public void SearchJava()
    {
        foreach (var java in JavaUtils.SearchJava())
            if (!Javas.Contains(java))
                Javas.Add(java);

        ActiveJava = Javas.Any() ? Javas[0] : null;

        OnPropertyChanged(nameof(Javas));

        _notificationService.NotifyWithoutContent(
            ResourceUtils.GetValue("Notifications", "_AddSearchedJavaT"),
            icon: "\uE73E");
    }

    [RelayCommand]
    public void RemoveFolder()
    {
        _gameService.RemoveMinecraftFolder(ActiveMinecraftFolder);
        OnPropertyChanged(nameof(IsMinecraftFoldersEmpty));
    }

    [RelayCommand]
    public void RemoveJava()
    {
        Javas.Remove(ActiveJava);
        ActiveJava = Javas.Any() ? Javas[0] : null;

        OnPropertyChanged(nameof(IsJavasEmpty));
    }

    [RelayCommand]
    public void ActivateCoresPage() => _navigationService.Parent.NavigateTo("CoresPage");

    [RelayCommand]
    public void OpenJavaMirrorsDialog()
    {
        _ = new JavaMirrorsDialog
        {
            XamlRoot = ShellPage._XamlRoot
        }.ShowAsync();
    }
}