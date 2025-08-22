using FluentLauncher.Infra.UI.Dialogs;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class InputInstanceIdDialog : ContentDialog, IDialogResultAware<string>
{
    InputInstanceIdDialogViewModel VM => (InputInstanceIdDialogViewModel)DataContext;

    string IDialogResultAware<string>.Result => VM.InstanceId;

    public InputInstanceIdDialog()
    {
        InitializeComponent();
    }
}
