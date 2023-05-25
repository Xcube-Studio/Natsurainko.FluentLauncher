using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Data;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class CurseForgeModInfoPage : Page
{
    public CurseForgeModInfoPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.DataContext = new ViewModels.Downloads.CurseForgeModInfoViewModel(e.Parameter as CurseForgeResourceData);
    }
}
