using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Components.FluentCore;

namespace Natsurainko.FluentLauncher.Views.Pages.Properties;

public sealed partial class Information : Page
{
    public Information()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.DataContext = new ViewModels.Pages.Properties.Information(e.Parameter as GameCore);
    }
}
