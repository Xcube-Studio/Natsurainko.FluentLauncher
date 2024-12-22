using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class DeleteInstanceDialog : ContentDialog
{
    DeleteInstanceDialogViewModel VM => (DeleteInstanceDialogViewModel)DataContext;

    public DeleteInstanceDialog()
    {
        InitializeComponent();
    }
}
