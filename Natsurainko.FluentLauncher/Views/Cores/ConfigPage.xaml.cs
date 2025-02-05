using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Cores;
using Nrk.FluentCore.Authentication;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class ConfigPage : Page, IBreadcrumbBarAware
{
    public string Route => "Config";

    ConfigViewModel VM => (ConfigViewModel)DataContext;

    public ConfigPage()
    {
        InitializeComponent();
    }

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var vm = (this.DataContext as ConfigViewModel)!;
        vm.inited = false;
        vm.InstanceConfig = null!;
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
