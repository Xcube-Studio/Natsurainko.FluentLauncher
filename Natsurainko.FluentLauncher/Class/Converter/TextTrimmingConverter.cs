using System;
using Windows.UI.Xaml.Data;

namespace Natsurainko.FluentLauncher.Class.Converter;

public class TextTrimmingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string obj = value.ToString();
        if (obj.Length > 19)
            return $"{obj.Substring(0, 18)}...";
        else return obj;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
