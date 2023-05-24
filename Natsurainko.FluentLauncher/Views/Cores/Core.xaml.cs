using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class Core : Page
{
    public Core()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.DataContext = new ViewModels.Cores.Core() { ContentDialog = e.Parameter as ContentDialog };
    }
}
