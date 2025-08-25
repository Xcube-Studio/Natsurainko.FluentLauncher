using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class LauncherPage : Page, IBreadcrumbBarAware
{
    public string Route => "Launcher";

    LauncherViewModel VM => (LauncherViewModel)DataContext;

    public LauncherPage()
    {
        InitializeComponent();
    }
}
