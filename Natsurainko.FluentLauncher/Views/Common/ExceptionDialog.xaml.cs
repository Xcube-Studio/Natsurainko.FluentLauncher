using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class ExceptionDialog : ContentDialog
{
    public ExceptionDialog(string errorMessage = "")
    {
        InitializeComponent();
        DataContext = new ExceptionDialogViewModel(errorMessage);
    }
}
