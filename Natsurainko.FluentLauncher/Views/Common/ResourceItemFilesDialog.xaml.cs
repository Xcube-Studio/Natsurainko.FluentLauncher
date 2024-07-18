using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class ResourceItemFilesDialog : ContentDialog
{
    public ResourceItemFilesDialog()
    {
        this.XamlRoot = MainWindow.XamlRoot;

        this.InitializeComponent();
    }
}
