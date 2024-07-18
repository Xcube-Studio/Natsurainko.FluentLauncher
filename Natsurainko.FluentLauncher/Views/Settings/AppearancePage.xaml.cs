using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class AppearancePage : Page, IBreadcrumbBarAware
{
    public string Route => "Appearance";

    public AppearancePage()
    {
        InitializeComponent();
    }
}
