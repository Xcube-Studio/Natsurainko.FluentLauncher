using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class AuthenticateDialog : ContentDialog
{
    AuthenticateDialogViewModel VM => (AuthenticateDialogViewModel)DataContext;

    public AuthenticateDialog()
    {
        InitializeComponent();
    }
}
