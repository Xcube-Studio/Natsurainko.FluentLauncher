using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Cores.Manage;

public sealed partial class SavePage : Page, IBreadcrumbBarAware
{
    public string Route => "Save";

    public SavePage()
    {
        this.InitializeComponent();
    }
}
