using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Natsurainko.FluentLauncher.Views.Cores.InstallNewCore;

public sealed partial class NewCorePage : Page
{
    public NewCorePage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.DataContext = new ViewModels.Cores.InstallNewCore.NewCoreViewModel() { ContentDialog = e.Parameter as ContentDialog };
    }
}
