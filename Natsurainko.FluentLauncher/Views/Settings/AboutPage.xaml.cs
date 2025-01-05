using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class AboutPage : Page, IBreadcrumbBarAware
{
    public string Route => "About";

    public AboutPage()
    {
        InitializeComponent();

#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
        this.Loaded += (object sender, Microsoft.UI.Xaml.RoutedEventArgs e) =>
        {
            this.CheckUpdateText.Text = "Check Update From Preview Channel";
        };
#endif
    }
}
