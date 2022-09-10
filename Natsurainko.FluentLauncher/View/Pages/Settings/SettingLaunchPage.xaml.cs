using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Shared.Mapping;
using Natsurainko.FluentLauncher.ViewModel.Pages.Settings;
using Natsurainko.Toolkits.Values;
using System;
using System.ComponentModel;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Pages.Settings;

public sealed partial class SettingLaunchPage : Page
{
    public SettingLaunchPageVM ViewModel { get; private set; }

    public SettingLaunchPage()
    {
        this.InitializeComponent();

        ViewModel = ViewModelBuilder.Build<SettingLaunchPageVM, Page>(this);
    }

    private async void FolderOpen_Click(object sender, RoutedEventArgs e)
    {
        var folderPicker = new FolderPicker
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.Desktop
        };

        folderPicker.FileTypeFilter.Add("*");
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            ViewModel.GameFolders.Add(folder.Path);
            ViewModel.CurrentGameFolder = folder.Path;

            ViewModel.OnPropertyChanged(new PropertyChangedEventArgs("GameFolders"));
        }
    }

    private void RemoveFolder_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.GameFolders.Remove(ViewModel.CurrentGameFolder);
        ViewModel.CurrentGameFolder = ViewModel.GameFolders.Any() ? ViewModel.GameFolders[0] : null;
    }

    private async void JavaOpen_Click(object sender, RoutedEventArgs e)
    {
        var openPicker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.Desktop
        };

        openPicker.FileTypeFilter.Add(".exe");

        var file = await openPicker.PickSingleFileAsync();

        if (file != null)
        {
            ViewModel.JavaRuntimes.Add(file.Path);
            ViewModel.CurrentJavaRuntime = file.Path;

            ViewModel.OnPropertyChanged(new PropertyChangedEventArgs("JavaRuntimes"));
        }
    }

    private void RemoveJava_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.JavaRuntimes.Remove(ViewModel.CurrentJavaRuntime);
        ViewModel.CurrentJavaRuntime = ViewModel.JavaRuntimes.Any() ? ViewModel.JavaRuntimes[0] : null;
    }

    private async void JavaSearch_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.JavaRuntimes.AddNotRepeating(await JavaHelper.SearchJavaRuntime());
        if (string.IsNullOrEmpty(ViewModel.CurrentJavaRuntime))
            ViewModel.CurrentJavaRuntime = ViewModel.JavaRuntimes[0];

        ViewModel.OnPropertyChanged(new PropertyChangedEventArgs(nameof(ViewModel.JavaRuntimes)));

        MainContainer.ShowInfoBarAsync("已将搜索到的 Java 运行时 添加至运行时列表");
    }
}
