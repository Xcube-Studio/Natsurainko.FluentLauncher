using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class ExceptionDialog : ContentDialog
{
    public ExceptionDialog(string errorMessage = "")
    {
        InitializeComponent();
        DataContext = new ExceptionDialogViewModel(errorMessage);
    }
}
