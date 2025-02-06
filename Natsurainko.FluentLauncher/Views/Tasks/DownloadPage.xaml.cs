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

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        itemsRepeater.ItemsSource = null;
    }
}
