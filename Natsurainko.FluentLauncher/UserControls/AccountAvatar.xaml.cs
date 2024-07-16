using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nrk.FluentCore.Authentication;

namespace Natsurainko.FluentLauncher.UserControls;

public sealed partial class AccountAvatar : UserControl
{
    public AccountAvatar()
    {
        this.InitializeComponent();
    }

    public Account Account
    {
        get { try { return (Account)GetValue(AccountProperty); } catch { return null; } }
        set { SetValue(AccountProperty, value); }
    }

    public static readonly DependencyProperty AccountProperty =
        DependencyProperty.Register("Account", typeof(Account), typeof(AccountAvatar), new PropertyMetadata(null));
}
