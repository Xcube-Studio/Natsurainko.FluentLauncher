using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.Classes.Enums;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

internal class AccountTypeToIsCheckedConverter : IValueConverter
{
    public AccountType? Type { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AccountType type)
            if (type.Equals(Type))
                return true;

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
