using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Data;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class CurseForgeModFile : Page
{
    public CurseForgeModFile()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        this.DataContext = new ViewModels.Downloads.CurseForgeModFile(e.Parameter as CurseForgeResourceData);
    }
}
