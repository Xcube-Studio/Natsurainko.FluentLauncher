using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    public string Route => "Settings";

    DefaultViewModel VM => (DefaultViewModel)DataContext;

#if ENABLE_LOAD_EXTENSIONS
    public bool ENABLE_LOAD_EXTENSIONS => true;
#else
    public bool ENABLE_LOAD_EXTENSIONS => false;
#endif

    public DefaultPage()
    {
        InitializeComponent();
    }
}
