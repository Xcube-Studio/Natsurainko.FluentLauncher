using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Data;
using Natsurainko.FluentLauncher.Services.Settings;

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

        DataContext = new ViewModels.Downloads.CurseForgeModFileViewModel(e.Parameter as CurseForgeResourceData, App.GetService<SettingsService>());
    }
}
