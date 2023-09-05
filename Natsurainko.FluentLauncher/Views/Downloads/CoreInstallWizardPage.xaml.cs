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
        InitializeComponent();
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index.Equals(0))
            ShellPage.ContentFrame.GoBack();
    }
}
