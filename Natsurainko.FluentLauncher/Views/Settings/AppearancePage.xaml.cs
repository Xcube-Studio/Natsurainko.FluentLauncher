using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class AppearancePage : Page, IBreadcrumbBarAware
{
    public string Route => "Appearance";

    AppearanceViewModel VM => (AppearanceViewModel)DataContext;

    public AppearancePage()
    {
        InitializeComponent();
    }
}
