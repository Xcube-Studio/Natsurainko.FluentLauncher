using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Natsurainko.FluentCore.Extension.Windows.Service;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.Toolkits.Values;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Guides;

public partial class Basic : SettingViewModel
{
    public Basic() : base() 
    {
        OnPropertyChanged("CanNext");
    }

    [ObservableProperty]
    private bool canNext;

    [ObservableProperty]
    private ObservableCollection<string> gameFolders;

    [ObservableProperty]
    private string currentGameFolder;

    [ObservableProperty]
    private ObservableCollection<string> javaRuntimes;

    [ObservableProperty]
    private string currentJavaRuntime;

    [ObservableProperty]
    private bool dropOpen;

    protected override void _OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CanNext))
            CanNext = 
                !string.IsNullOrEmpty(CurrentGameFolder) &&
                !string.IsNullOrEmpty(CurrentJavaRuntime) &&
                Directory.Exists(CurrentGameFolder) &&
                File.Exists(CurrentJavaRuntime);

        if (e.PropertyName == nameof(CanNext))
            WeakReferenceMessenger.Default.Send(new GuideNavigationMessage()
            {
                CanNext = canNext,
                NextPage = typeof(Views.Pages.Guides.Account)
            });
    }
}

public partial class Basic
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

        DropOpen = true;
    }
}