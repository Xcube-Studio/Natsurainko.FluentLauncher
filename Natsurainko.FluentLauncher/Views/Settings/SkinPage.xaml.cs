using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class SkinPage : Page, IBreadcrumbBarAware
{
    public string Route => "Skin";

    public SkinPage()
    {
        this.InitializeComponent();
    }
}
