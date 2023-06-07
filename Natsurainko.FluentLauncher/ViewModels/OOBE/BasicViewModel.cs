using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Natsurainko.FluentCore.Extension.Windows.Service;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.Toolkits.Values;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

partial class BasicViewModel : SettingsViewModelBase, ISettingsViewModel
{
    #region Settings

    [SettingsProvider]
    private readonly SettingsService _settingsService;

    [BindToSetting(Path = nameof(SettingsService.GameFolders))]
    public ObservableCollection<string> GameFolders { get; private set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CurrentGameFolder))]
    private string currentGameFolder;

    [BindToSetting(Path = nameof(SettingsService.JavaRuntimes))]
    public ObservableCollection<string> JavaRuntimes { get; private set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CurrentJavaRuntime))]
    private string currentJavaRuntime;

    #endregion

    [ObservableProperty]
    private bool dropDownOpen;


    public BasicViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        (this as ISettingsViewModel).InitializeSettings();
    }

    partial void OnCurrentGameFolderChanged(string oldValue, string newValue)
        => UpdateNavigationStatus();

    partial void OnCurrentJavaRuntimeChanged(string oldValue, string newValue)
        => UpdateNavigationStatus();

    /// <summary>
    /// Validate CurrentGameFolder and CurrentJavaRuntime to determine if navigation to the next step is allowed.
    /// </summary>
    void UpdateNavigationStatus()
    {
        bool canContinue =
            !string.IsNullOrEmpty(CurrentGameFolder) &&
            !string.IsNullOrEmpty(CurrentJavaRuntime) &&
            Directory.Exists(CurrentGameFolder) &&
            File.Exists(CurrentJavaRuntime);

        WeakReferenceMessenger.Default.Send(new GuideNavigationMessage()
        {
            CanNext = canContinue,
            NextPage = typeof(Views.OOBE.AccountPage)
        });
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
                //OnPropertyChanged(nameof(GameFolders));

                CurrentGameFolder = folder.Path;
            });
    });

    [RelayCommand]
    public Task BrowserJava() => Task.Run(async () =>
    {
        var filePicker = new FileOpenPicker();

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

        filePicker.FileTypeFilter.Add(".exe");
        var file = await filePicker.PickSingleFileAsync();

        if (file != null)
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                JavaRuntimes.Add(file.Path);
                OnPropertyChanged(nameof(JavaRuntimes));

                CurrentJavaRuntime = file.Path;
            });
    });

    [RelayCommand]
    public void SearchJava()
    {
        JavaRuntimes.AddNotRepeating(JavaHelper.SearchJavaRuntime());
        CurrentJavaRuntime = JavaRuntimes.Any() ? JavaRuntimes[0] : null;

        OnPropertyChanged(nameof(JavaRuntimes));

        DropDownOpen = true;
        MessageService.Show("Added the search Java to the runtime list");
    }
}