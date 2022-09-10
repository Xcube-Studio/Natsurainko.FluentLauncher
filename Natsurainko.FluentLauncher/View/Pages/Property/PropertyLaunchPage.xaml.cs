using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.ViewModel.Pages.Property;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Natsurainko.FluentLauncher.View.Pages.Property;

public sealed partial class PropertyLaunchPage : Page
{
    public PropertyLaunchPageVM ViewModel { get; set; }

    public PropertyLaunchPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel = ViewModelBuilder.Build<PropertyLaunchPageVM, Page>(this);
        ViewModel.Set((e.Parameter as GameCoreViewData).Data);
    }
}
