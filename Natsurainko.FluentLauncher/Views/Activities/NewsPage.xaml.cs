using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Activities;

namespace Natsurainko.FluentLauncher.Views.Activities;

public sealed partial class NewsPage : Page
{
    public NewsPage()
    {
        InitializeComponent();
        this.DataContext = App.Services.GetService<NewsViewModel>();
    }
}
