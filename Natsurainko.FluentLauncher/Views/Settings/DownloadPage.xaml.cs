using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class DownloadPage : Page, IBreadcrumbBarAware
{
    public string Route => "Download";

    DownloadViewModel VM => (DownloadViewModel)DataContext;

    public DownloadPage()
    {
        InitializeComponent();
    }
}
