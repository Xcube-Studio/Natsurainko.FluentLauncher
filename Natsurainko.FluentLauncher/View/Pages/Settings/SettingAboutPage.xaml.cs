using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.ViewModel.Pages.Settings;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Pages.Settings;

public sealed partial class SettingAboutPage : Page
{
    public SettingAboutPage()
    {
        this.InitializeComponent();

        ViewModelBuilder.Build<SettingAboutPageVM, Page>(this);
    }

    private async void AuthorExpanderEx_Click(object sender, Richasy.ExpanderEx.Uwp.ExpanderExClickEventArgs e)
        => await Launcher.LaunchUriAsync(new Uri("https://github.com/natsurainko"));

    private async void SourceExpanderEx_Click(object sender, Richasy.ExpanderEx.Uwp.ExpanderExClickEventArgs e)
        => await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Fluent-Launcher"));

    private async void Button_Click(object sender, RoutedEventArgs e)
        => await Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?productid=9P4NQQXQ942P"));
}
