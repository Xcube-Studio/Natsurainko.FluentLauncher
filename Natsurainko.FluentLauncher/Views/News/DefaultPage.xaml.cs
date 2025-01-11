using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.News;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    public string Route => "News";

    ViewModels.News.DefaultViewModel VM => (ViewModels.News.DefaultViewModel)DataContext;

    public DefaultPage()
    {
        InitializeComponent();
    }
}
