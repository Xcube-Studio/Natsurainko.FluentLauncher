using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Nrk.FluentCore.Classes.Datas.Launch;

namespace Natsurainko.FluentLauncher.Views.Cores.Properties;

public sealed partial class InformationPage : Page
{
    public InformationPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.DataContext = new ViewModels.Cores.Properties.InformationViewModel(e.Parameter as GameInfo);
    }
}
