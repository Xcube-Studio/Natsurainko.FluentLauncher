using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class AboutPage : Page, IBreadcrumbBarAware
{
    public string Route => "About";

    AboutViewModel VM => (AboutViewModel)DataContext;

    public AboutPage()
    {
        InitializeComponent();

#if FLUENT_LAUNCHER_DEV_CHANNEL
        AboutCard.IsEnabled = false;
#elif FLUENT_LAUNCHER_PREVIEW_CHANNEL
        this.Loaded += (_, _) =>
        {
            this.CheckUpdateText.Text = LocalizedStrings.Settings_AboutPage__UpdateFromPreviewChannel;
        };
#endif
    }
}