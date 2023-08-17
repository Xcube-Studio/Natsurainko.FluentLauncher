using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Classes.Enums;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

internal class AccountInfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AccountType accountType)
        {
            var account = ResourceUtils.GetValue("Converters", "_Account");

            return accountType switch
            {
                AccountType.Microsoft => ResourceUtils.GetValue("Converters", "_Microsoft") + account,
                AccountType.Yggdrasil => ResourceUtils.GetValue("Converters", "_Yggdrasil") + account,
                _ => ResourceUtils.GetValue("Converters", "_Offline") + account,
            };
        }

        if (value is not Account) return null;
        if (parameter is not string NeedProperty) return null;

        return NeedProperty switch
        {
            "LastRefreshTime" => value is MicrosoftAccount microsoftAccount
                ? microsoftAccount.LastRefreshTime
                : null,
            "LastRefreshVisibility" => value is MicrosoftAccount
                ? Visibility.Visible
                : Visibility.Collapsed,
            "YggdrasilServerUrl" => value is YggdrasilAccount yggdrasilAccount
                ? yggdrasilAccount.YggdrasilServerUrl
                : null,
            "YggdrasilServerUrlVisibility" => value is YggdrasilAccount
                ? Visibility.Visible
                : Visibility.Collapsed,
            _ => null
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
