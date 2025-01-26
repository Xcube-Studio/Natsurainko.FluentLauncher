using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class SavePage : Page, IBreadcrumbBarAware
{
    public string Route => "Save";

    SaveViewModel VM => (SaveViewModel)DataContext;

    public SavePage()
    {
        InitializeComponent();
    }
}
