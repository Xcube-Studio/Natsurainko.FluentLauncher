using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class DownloadPage : Page, IBreadcrumbBarAware
{
    public string Route => "Download";

    public DownloadPage()
    {
        InitializeComponent();
    }
}
