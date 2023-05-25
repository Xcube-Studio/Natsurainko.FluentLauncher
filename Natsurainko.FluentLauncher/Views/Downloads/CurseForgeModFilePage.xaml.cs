using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Data;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class CurseForgeModFilePage : Page
{
    public CurseForgeModFilePage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        this.DataContext = new ViewModels.Downloads.CurseForgeModFileViewModel(e.Parameter as CurseForgeResourceData);
    }
}
