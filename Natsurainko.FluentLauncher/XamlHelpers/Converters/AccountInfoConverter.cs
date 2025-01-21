﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Authentication;
using System;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

internal partial class AccountInfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AccountType accountType)
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

        if (value is not Account) return null;
        if (parameter is not string NeedProperty) return null;

        return NeedProperty switch
        {
            "LastRefreshTime" => value is MicrosoftAccount microsoftAccount
                ? microsoftAccount.LastRefreshTime.ToString()
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
