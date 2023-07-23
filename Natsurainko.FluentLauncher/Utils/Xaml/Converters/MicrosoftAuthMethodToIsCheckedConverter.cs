using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

internal class MicrosoftAuthMethodToIsCheckedConverter : IValueConverter
{
    public MicrosoftAuthMethod? Method { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MicrosoftAuthMethod method)
            if (method.Equals(Method))
                return true;

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
