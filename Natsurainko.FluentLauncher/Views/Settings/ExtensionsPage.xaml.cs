using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

#if ENABLE_LOAD_EXTENSIONS

public sealed partial class ExtensionsPage : Page, IBreadcrumbBarAware
{
    public string Route => "Extensions";

    ExtensionsViewModel VM => (ExtensionsViewModel)DataContext;

    public ExtensionsPage()
    {
        this.InitializeComponent();
    }
}

#endif