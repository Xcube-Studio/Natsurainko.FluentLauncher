using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Natsurainko.FluentLauncher.Class.Converter;

public class ValueIsNullVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value == null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
