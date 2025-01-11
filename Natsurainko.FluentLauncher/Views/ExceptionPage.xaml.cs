using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class ExceptionPage : Page
{
    ExceptionDialogViewModel VM => (ExceptionDialogViewModel)DataContext;

    public ExceptionPage(string errorMessage = "")
    {
        InitializeComponent();
        DataContext = new ExceptionDialogViewModel(errorMessage);
    }
}
