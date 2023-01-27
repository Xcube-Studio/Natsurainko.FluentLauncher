using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Natsurainko.FluentLauncher.Views.Pages.Installations;

public sealed partial class Core : Page
{
    public Core()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.DataContext = new ViewModels.Pages.Installations.Core() { ContentDialog = e.Parameter as ContentDialog };
    }
}
