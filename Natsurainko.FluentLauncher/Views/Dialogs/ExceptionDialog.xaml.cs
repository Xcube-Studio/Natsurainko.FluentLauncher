using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

// This dialog is not managed by the DI framework because it is called in App.xaml.cs, where a scope is not available
public sealed partial class ExceptionDialog : ContentDialog
{
    ExceptionDialogViewModel VM => (ExceptionDialogViewModel)DataContext;

    public ExceptionDialog(string errorMessage = "")
    {
        InitializeComponent();
        DataContext = new ExceptionDialogViewModel(errorMessage);
    }
}
