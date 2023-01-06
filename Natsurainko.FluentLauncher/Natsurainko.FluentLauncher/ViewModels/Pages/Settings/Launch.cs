using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentCore.Extension.Windows.Service;
using Natsurainko.FluentLauncher.Views.Pages;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Natsurainko.Toolkits.Values;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using System.Linq;
using Natsurainko.FluentLauncher.Components.Mvvm;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Settings;

public partial class Launch : SettingViewModel
{
    public Launch() : base() { }

    protected override void _OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(RemoveJavaVisibility))
            RemoveFolderVisibility = string.IsNullOrEmpty(CurrentGameFolder)
                ? Visibility.Collapsed
                : Visibility.Visible;

        if (e.PropertyName != nameof(RemoveJavaVisibility))
            RemoveJavaVisibility = string.IsNullOrEmpty(CurrentJavaRuntime)
                ? Visibility.Collapsed
                : Visibility.Visible;
    }
}

public partial class Launch
{
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
                GameFolders.Add(folder.Path);
                OnPropertyChanged(nameof(GameFolders));

                CurrentGameFolder = folder.Path;
            });
    });

    [RelayCommand]
    public Task BrowserJava() => Task.Run(async () =>
    {
        var filePicker = new FileOpenPicker();

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

        filePicker.FileTypeFilter.Add("*");
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

        MainContainer.ShowMessagesAsync("Added the search Java to the runtime list");
    }

    [RelayCommand]
    public void RemoveFolder()
    {
        GameFolders.Remove(CurrentGameFolder);
        CurrentGameFolder = GameFolders.Any() ? GameFolders[0] : null;

        OnPropertyChanged(nameof(GameFolders));
    }

    [RelayCommand]
    public void RemoveJava()
    {
        JavaRuntimes.Remove(CurrentJavaRuntime);
        CurrentJavaRuntime = JavaRuntimes.Any() ? JavaRuntimes[0] : null;

        OnPropertyChanged(nameof(JavaRuntimes));
    }
}

public partial class Launch
{
    [ObservableProperty]
    private Visibility removeFolderVisibility;

    [ObservableProperty]
    private Visibility removeJavaVisibility;

    [ObservableProperty]
    private ObservableCollection<string> gameFolders;

    [ObservableProperty]
    private string currentGameFolder;

    [ObservableProperty]
    private ObservableCollection<string> javaRuntimes;

    [ObservableProperty]
    private string currentJavaRuntime;

    [ObservableProperty]
    private int javaVirtualMachineMemory;

    [ObservableProperty]
    private bool enableAutoMemory;

    [ObservableProperty]
    private string gameWindowTitle;

    [ObservableProperty]
    private int gameWindowWidth;

    [ObservableProperty]
    private int gameWindowHeight;

    [ObservableProperty]
    private string gameServerAddress;

    [ObservableProperty]
    private bool enableFullScreen;

    [ObservableProperty]
    private bool enableIndependencyCore;
}