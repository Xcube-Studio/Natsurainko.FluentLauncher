using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.ViewModel.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Dialogs;

public sealed partial class AuthenticatorTypeDialog : ContentDialog
{
    public AuthenticatorTypeDialogVM ViewModel { get; private set; }

    public bool Canceled { get; private set; }

    public AuthenticatorTypeDialog()
    {
        this.InitializeComponent();

        ViewModel = ViewModelBuilder.Build<AuthenticatorTypeDialogVM, ContentDialog>(this);
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        Canceled = false;
        this.Hide();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Canceled = true;
        this.Hide();

        MainContainer.ShowInfoBarAsync(
            ConfigurationManager.AppSettings.CurrentLanguage.GetString("SettingAccountPage_AddCancel"),
            severity: InfoBarSeverity.Informational);
    }
}
