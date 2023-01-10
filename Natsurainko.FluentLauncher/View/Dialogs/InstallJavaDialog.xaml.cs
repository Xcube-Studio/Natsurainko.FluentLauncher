using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.ViewModel.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Dialogs;

public sealed partial class InstallJavaDialog : ContentDialog
{
    public InstallJavaDialogVM ViewModel { get; set; }

    public InstallJavaDialog()
    {
        this.InitializeComponent();

        ViewModel = ViewModelBuilder.Build<InstallJavaDialogVM, ContentDialog>(this);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();

        MainContainer.ShowInfoBarAsync(
            ConfigurationManager.AppSettings.CurrentLanguage.GetString("InstallJavaDialog_InstallCancelled"),
            severity: InfoBarSeverity.Informational);
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();
        JavaInstallerViewData.CreateJavaInstallProcess(ViewModel.CurrentUrl.Value.Value, ViewModel.CurrentDownloadSource, sender as Control);
    }
}
