using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Downloads;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class ResourceDefaultPage : Page
{
    ResourceDefaultViewModel VM => (ResourceDefaultViewModel)DataContext;

    public ResourceDefaultPage()
    {
        InitializeComponent();
    }
}
