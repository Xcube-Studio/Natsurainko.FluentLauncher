using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.News;

public sealed partial class NotePage : Page, IBreadcrumbBarAware
{
    public string Route => "Note";

    public NotePage()
    {
        this.InitializeComponent();
    }
}
