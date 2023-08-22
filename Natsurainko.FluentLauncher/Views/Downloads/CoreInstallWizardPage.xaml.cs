using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.ViewModels.Downloads;
using Nrk.FluentCore.Classes.Datas.Download;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class CoreInstallWizardPage : Page
{
    public CoreInstallWizardPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var manifestItem = e.Parameter as VersionManifestItem;

        BreadcrumbBar.ItemsSource = new string[] { "核心安装向导", manifestItem.Id };
        this.DataContext = new CoreInstallWizardViewModel(manifestItem);
    }
}
