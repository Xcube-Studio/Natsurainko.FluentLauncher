using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Settings;

namespace Natsurainko.FluentLauncher.Views.Settings;

public sealed partial class DownloadPage : Page
{
    public DownloadPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetService<DownloadViewModel>();
    }
}
