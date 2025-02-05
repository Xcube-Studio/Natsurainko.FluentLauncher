using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.OOBE;
using Nrk.FluentCore.Authentication;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class AccountPage : Page
{
    OOBEViewModel VM => (OOBEViewModel)DataContext;

    public AccountPage()
    {
        InitializeComponent();
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var vm = this.DataContext as OOBEViewModel;

        WeakReferenceMessenger.Default.Register<ActiveAccountChangedMessage>(vm!, (r, m) =>
        {
            OOBEViewModel vm = (r as OOBEViewModel)!;

            vm.processingActiveAccountChangedMessage = true;
            vm.ActiveAccount = m.Value;
            vm.processingActiveAccountChangedMessage = false;
        });

        void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Unregister<ActiveAccountChangedMessage>(vm!);
        }

        this.Unloaded += Page_Unloaded;
    }

    #region Converters Methods

    internal static string GetAccountTypeName(AccountType accountType)
    {
        string account = LocalizedStrings.Converters__Account;

        if (!ApplicationLanguages.PrimaryLanguageOverride.StartsWith("zh-"))
            account = " " + account;

        return accountType switch
        {
            AccountType.Microsoft => LocalizedStrings.Converters__Microsoft + account,
            AccountType.Yggdrasil => LocalizedStrings.Converters__Yggdrasil + account,
            _ => LocalizedStrings.Converters__Offline + account,
        };
    }

    internal static string TryGetYggdrasilServerName(Account account)
    {
        if (account is YggdrasilAccount yggdrasilAccount)
        {
            if (yggdrasilAccount.MetaData.TryGetValue("server_name", out var serverName))
                return serverName;
        }

        return string.Empty;
    }

    #endregion
}
