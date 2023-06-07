using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.OOBE;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class GetStartedPage : Page
{
    public GetStartedPage()
    {
        InitializeComponent();
        DataContext = App.GetService<GetStartedViewModel>();
    }
}
