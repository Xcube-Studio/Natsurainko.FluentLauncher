using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Windows.ApplicationModel;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

partial class AboutViewModel : ObservableObject
{
    [ObservableProperty]
    private string version = string.Format("{0}.{1}.{2}.{3}",
            Package.Current.Id.Version.Major,
            Package.Current.Id.Version.Minor,
            Package.Current.Id.Version.Build,
            Package.Current.Id.Version.Revision);

#if DEBUG
    [ObservableProperty]
    private string edition = "Debug Edition";
#else
    [ObservableProperty]
    private string edition = "Release Edition";
#endif

    [RelayCommand]
    public async void CheckUpdate()
        => await Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?productid=9P4NQQXQ942P"));

    [RelayCommand]
    public async void OpenGit()
        => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Fluent-Launcher"));

    [RelayCommand]
    public async void OpenAuthor()
        => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/graphs/contributors"));
}