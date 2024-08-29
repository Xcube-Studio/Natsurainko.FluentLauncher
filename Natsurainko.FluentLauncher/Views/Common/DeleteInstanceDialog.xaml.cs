using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class DeleteInstanceDialog : ContentDialog
{
    public DeleteInstanceDialog()
    {
        this.XamlRoot = MainWindow.XamlRoot;

        this.InitializeComponent();
    }
}
