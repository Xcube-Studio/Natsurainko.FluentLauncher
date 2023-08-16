using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Common;
using System;
using Windows.ApplicationModel;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class AboutViewModel : ObservableObject
{
    [ObservableProperty]
    private string version = string.Format("{0}.{1}.{2}.{3}",
            Package.Current.Id.Version.Major,
            Package.Current.Id.Version.Minor,
            Package.Current.Id.Version.Build,
            Package.Current.Id.Version.Revision);

#if DEBUG
    [ObservableProperty]
    private string edition = ResourceUtils.GetValue("Settings", "AboutPage", "_Debug");
#else
    [ObservableProperty]
    private string edition = ResourceUtils.GetValue("Settings", "AboutPage", "_Release");
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

    [RelayCommand]
    public async void DisplayCopyrightLicense()
    {
        await new CopyrightLicenseDialog
        {
            XamlRoot = ShellPage._XamlRoot
        }.ShowAsync();
    }
}