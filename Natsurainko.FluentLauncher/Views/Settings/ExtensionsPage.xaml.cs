#if ENABLE_LOAD_EXTENSIONS
using CommunityToolkit.WinUI.Controls;
using FluentLauncher.Infra.ExtensionHost.Extensions;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class ExtensionsPage : Page, IBreadcrumbBarAware
{
    public string Route => "Extensions";

    ExtensionsViewModel VM => (ExtensionsViewModel)DataContext;

    public ExtensionsPage()
    {
        this.InitializeComponent();
    }

    private void SettingsCard_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        SettingsCard settingsCard = (SettingsCard)sender;

        if (settingsCard.DataContext is IExtension extension)
        {
            settingsCard.Header = extension.Name;
            settingsCard.Description = extension.Description;
        }
    }
}

#else
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class ExtensionsPage : Page
{
    public ExtensionsPage() => this.InitializeComponent();

    private void SettingsCard_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) { }
}

#endif