using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class DownloadResourceDialog : ContentDialog
{
    public DownloadResourceDialog()
    {
        this.XamlRoot = MainWindow.XamlRoot;

        this.InitializeComponent();
    }
}
