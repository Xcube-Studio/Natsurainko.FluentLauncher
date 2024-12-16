using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class ExceptionDialog : ContentDialog
{
    ExceptionDialogViewModel VM => (ExceptionDialogViewModel)DataContext;

    public ExceptionDialog(string errorMessage = "")
    {
        XamlRoot = MainWindow.XamlRoot;

        InitializeComponent();
        DataContext = new ExceptionDialogViewModel(errorMessage);
    }
}
