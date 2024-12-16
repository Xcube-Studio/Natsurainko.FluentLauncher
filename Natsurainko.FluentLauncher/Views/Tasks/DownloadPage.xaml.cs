using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Tasks;

namespace Natsurainko.FluentLauncher.Views.Tasks;

public sealed partial class DownloadPage : Page
{
    DownloadViewModel VM => (DownloadViewModel)DataContext;

    public DownloadPage()
    {
        InitializeComponent();
    }
}
