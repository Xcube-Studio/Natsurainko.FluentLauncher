using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

internal class AccountInfoConverter : IValueConverter
{
    public string NeedProperty { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Account) return null;
        if (NeedProperty == null) return null;

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
