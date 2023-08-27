using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Utils;
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

        BreadcrumbBar.ItemsSource = new string[]
        {
            ResourceUtils.GetValue("Downloads", "CoreInstallWizardPage", "_BreadcrumbBar_First"),
            manifestItem.Id
        };

        this.DataContext = new CoreInstallWizardViewModel(manifestItem);
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index.Equals(0))
            ShellPage.ContentFrame.GoBack();
    }
}
