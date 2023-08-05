using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Downloads;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class DownloadsPage : Page
{
    public DownloadsPage()
    {
        this.InitializeComponent();
        this.DataContext = new DownloadsViewModel();
    }
}
