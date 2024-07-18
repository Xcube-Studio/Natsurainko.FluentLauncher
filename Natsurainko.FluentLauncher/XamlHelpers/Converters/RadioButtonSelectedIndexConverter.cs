using Microsoft.UI.Xaml.Data;
using System;

namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

public class RadioButtonSelectedIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var currentIndex = (int)parameter;
        var requestIndex = (int)value;

        return currentIndex == requestIndex;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
