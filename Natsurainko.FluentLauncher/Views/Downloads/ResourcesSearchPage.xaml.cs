using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Classes.Data.Download;
using Natsurainko.FluentLauncher.ViewModels.Downloads;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class ResourcesSearchPage : Page
{
    public ResourcesSearchPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.DataContext = new ResourcesSearchViewModel(e.Parameter as ResourceSearchData);
    }
}
