using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class AboutViewModel : ObservableObject
{
    public string Version => App.Version.GetVersionString();

#if DEBUG
    [ObservableProperty]
    public partial string Edition { get; set; } = ResourceUtils.GetValue("Settings", "AboutPage", "_Debug");
#else
    [ObservableProperty]
    public partial string Edition { get; set; } = ResourceUtils.GetValue("Settings", "AboutPage", "_Release");
#endif

    [RelayCommand]
    public async Task CheckUpdate()
        => await Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?productid=9P4NQQXQ942P"));

    [RelayCommand]
    public async Task OpenGit()
        => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Fluent-Launcher"));

    [RelayCommand]
    public async Task OpenAuthor()
        => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/graphs/contributors"));

    [RelayCommand]
    public async Task OpenLicense()
        => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/blob/main/LICENSE"));

}