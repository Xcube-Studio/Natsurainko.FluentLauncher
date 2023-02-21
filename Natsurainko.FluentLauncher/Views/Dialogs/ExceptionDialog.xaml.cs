using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class ExceptionDialog : ContentDialog
{
    public string ErrorMessage { get; private set; }
    public ExceptionDialog(string errorMessage = "")
    {
        InitializeComponent();
        ErrorMessage = errorMessage;
    }
}
