using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;
using Nrk.FluentCore.Authentication;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class ConfirmProfilePage : Page
{
    ConfirmProfileViewModel VM => (ConfirmProfileViewModel)DataContext;

    public ConfirmProfilePage()
    {
        InitializeComponent();
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ItemsView.SelectionChanged += ItemsView_SelectionChanged;
        ItemsView.Select(0);
    }

    private void ItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        VM.SelectedAccount = (Account)sender.SelectedItem!;
    }

    #region Converter Methods

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
