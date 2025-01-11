using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Common;

internal sealed partial class AddVmArgumentDialog : ContentDialog
{
    AddVmArgumentDialogViewModel VM => (AddVmArgumentDialogViewModel)DataContext;

    public AddVmArgumentDialog()
    {
        InitializeComponent();
        this.RequestedTheme = (ElementTheme)_settingsService.DisplayTheme;
    }
}
