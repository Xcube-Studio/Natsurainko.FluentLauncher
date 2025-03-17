using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;
using Nrk.FluentCore.Authentication;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

internal sealed partial class SwitchAccountDialog : ContentDialog
{
    SwitchAccountDialogViewModel VM => (SwitchAccountDialogViewModel)DataContext;

    public SwitchAccountDialog(SettingsService settingsService)
    {
        InitializeComponent();
        RequestedTheme = (ElementTheme)settingsService.DisplayTheme;
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
