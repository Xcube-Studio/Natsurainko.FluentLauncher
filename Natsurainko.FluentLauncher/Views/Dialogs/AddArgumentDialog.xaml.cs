using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

internal sealed partial class AddArgumentDialog : ContentDialog
{
    AddArgumentDialogViewModel VM => (AddArgumentDialogViewModel)DataContext;

    public AddArgumentDialog()
    {
        InitializeComponent();
    }
}
