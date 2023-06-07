using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.Win32;
using Natsurainko.FluentCore.Extension.Windows.Service;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.Toolkits.Values;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

partial class LaunchViewModel : SettingsViewModelBase, ISettingsViewModel
{
    // TODO: Disable game window size settings when full screen mode is on

    [SettingsProvider]
    private readonly SettingsService _settingsService;

    #region Settings

    // TODO: [BindToSetting(Path = nameof(SettingsService.GameFolders))]
    public ObservableCollection<string> GameFolders => _settingsService.GameFolders;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGameFoldersEmpty))]
    [BindToSetting(Path = nameof(SettingsService.CurrentGameFolder))]
    private string currentGameFolder;

    // TODO: [BindToSetting(Path = nameof(SettingsService.JavaRuntimes))]
    public ObservableCollection<string> JavaRuntimes => _settingsService.JavaRuntimes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsJavaRuntimesEmpty))]
    [BindToSetting(Path = nameof(SettingsService.CurrentJavaRuntime))]
    private string currentJavaRuntime;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.JavaVirtualMachineMemory))]
    private int javaVirtualMachineMemory;

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

    public bool IsGameFoldersEmpty => GameFolders.Count == 0;

    public bool IsJavaRuntimesEmpty => JavaRuntimes.Count == 0;

    public LaunchViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        (this as ISettingsViewModel).InitializeSettings();
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
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (GameFolders.Contains(folder.Path))
                {
                    MessageService.Show("This folder already exists");
                    return;
                }

                GameFolders.Add(folder.Path);
                CurrentGameFolder = folder.Path;

                OnPropertyChanged(nameof(IsGameFoldersEmpty));
            });

    });

    [RelayCommand]
    public void BrowserJava()
    {
        var openFileDialog = new OpenFileDialog();
        openFileDialog.Multiselect = false;
        openFileDialog.Filter = "Javaw Executable File|javaw.exe|Java Executable File|java.exe";

        if (openFileDialog.ShowDialog().GetValueOrDefault(false))
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                JavaRuntimes.Add(openFileDialog.FileName);
                CurrentJavaRuntime = openFileDialog.FileName;

                OnPropertyChanged(nameof(IsJavaRuntimesEmpty));
            });
    }

    [RelayCommand]
    public void SearchJava()
    {
        JavaRuntimes.AddNotRepeating(JavaHelper.SearchJavaRuntime());
        CurrentJavaRuntime = JavaRuntimes.Any() ? JavaRuntimes[0] : null;

        OnPropertyChanged(nameof(JavaRuntimes));

        MessageService.Show("Added the search Java to the runtime list");
    }

    [RelayCommand]
    public void RemoveFolder()
    {
        GameFolders.Remove(CurrentGameFolder);
        CurrentGameFolder = GameFolders.Any() ? GameFolders[0] : null;

        OnPropertyChanged(nameof(IsGameFoldersEmpty));
    }

    [RelayCommand]
    public void RemoveJava()
    {
        JavaRuntimes.Remove(CurrentJavaRuntime);
        CurrentJavaRuntime = JavaRuntimes.Any() ? JavaRuntimes[0] : null;

        OnPropertyChanged(nameof(IsJavaRuntimesEmpty));
    }

    [RelayCommand]
    void ActivateCoresPage()
    {
        Views.ShellPage.ContentFrame.Navigate(typeof(Views.Cores.CoresPage));
    }

}