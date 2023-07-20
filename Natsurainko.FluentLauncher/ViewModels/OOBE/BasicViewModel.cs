using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.Toolkits.Values;
using Nrk.FluentCore.Utils;
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

    [BindToSetting(Path = nameof(SettingsService.MinecraftFolders))]
    public ObservableCollection<string> MinecraftFolders { get; private set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.ActiveMinecraftFolder))]
    private string activeMinecraftFolder;

    [BindToSetting(Path = nameof(SettingsService.Javas))]
    public ObservableCollection<string> Javas { get; private set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.ActiveJava))]
    private string activeJava;

    #endregion

    private readonly GameService _gameService;

    [ObservableProperty]
    private bool dropDownOpen;

    public BasicViewModel(SettingsService settingsService, GameService gameService)
    {
        _settingsService = settingsService;
        _gameService = gameService;

        (this as ISettingsViewModel).InitializeSettings();
    }

    partial void OnActiveMinecraftFolderChanged(string oldValue, string newValue) => UpdateNavigationStatus();

    partial void OnActiveJavaChanged(string oldValue, string newValue) => UpdateNavigationStatus();

    /// <summary>
    /// Validate CurrentGameFolder and CurrentJavaRuntime to determine if navigation to the next step is allowed.
    /// </summary>
    void UpdateNavigationStatus()
    {
        bool canContinue =
            !string.IsNullOrEmpty(ActiveMinecraftFolder) &&
            !string.IsNullOrEmpty(ActiveJava) &&
            Directory.Exists(ActiveMinecraftFolder) &&
            File.Exists(ActiveJava);

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
                if (MinecraftFolders.Contains(folder.Path))
                {
                    MessageService.Show("This folder already exists");
                    return;
                }

                MinecraftFolders.Add(folder.Path);
                _gameService.ActivateMinecraftFolder(folder.Path);
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
                Javas.Add(file.Path);
                OnPropertyChanged(nameof(Javas));

                ActiveJava = file.Path;
            });
    });

    [RelayCommand]
    public void SearchJava()
    {
        Javas.AddNotRepeating(JavaUtils.SearchJava());
        ActiveJava = Javas.Any() ? Javas[0] : null;

        OnPropertyChanged(nameof(Javas));

        DropDownOpen = true;
        MessageService.Show("Added the search Java to the runtime list");
    }
}