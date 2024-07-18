using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class AddVmArgumentDialog : ContentDialog
{
    public AddVmArgumentDialog()
    {
        this.XamlRoot = MainWindow.XamlRoot;

        this.InitializeComponent();
    }
}
