using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Pages;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class LoggerPage : Page
{
    LoggerViewModel VM => (LoggerViewModel)DataContext;

    public LoggerPage()
    {
        InitializeComponent();
    }
}
