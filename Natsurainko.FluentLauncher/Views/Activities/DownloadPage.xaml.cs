using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Activities;

namespace Natsurainko.FluentLauncher.Views.Activities;

public sealed partial class DownloadPage : Page
{
    public DownloadPage()
    {
        InitializeComponent();
        this.DataContext = App.GetService<DownloadViewModel>();
    }
}
