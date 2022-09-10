using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.ViewModel.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Dialogs;

public sealed partial class LoginDialog : ContentDialog
{
    public LoginDialogVM ViewModel { get; private set; }

    public string AccessCode { get; private set; }

    public LoginDialog(int type, string accessCode)
    {
        this.InitializeComponent();
        AccessCode = accessCode;

        ViewModel = ViewModelBuilder.Build<LoginDialogVM, LoginDialog>(this);
        ViewModel.SetType(type);
    }

    public void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        switch (ViewModel.CurrentAuthenticatorType)
        {
            case 0:
                ViewModel.OfflineAuthenticate();
                break;
            case 1:
                ViewModel.MicrosoftAuthenticate();
                break;
            case 2:
                ViewModel.YggdrasilAuthenticate();
                break;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();
        MainContainer.ShowInfoBarAsync($"已取消添加账户：", string.Empty, severity: InfoBarSeverity.Informational);
    }
}
