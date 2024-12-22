using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class AddVmArgumentDialog : ContentDialog
{
    AddVmArgumentDialogViewModel VM => (AddVmArgumentDialogViewModel)DataContext;

    public AddVmArgumentDialog()
    {
        InitializeComponent();
    }
}
