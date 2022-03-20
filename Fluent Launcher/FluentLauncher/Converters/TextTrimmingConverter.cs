using System;
using Windows.UI.Xaml.Data;

namespace FluentLauncher.Converters
{
    public class TextTrimmingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string obj = value.ToString();
            if (obj.Length > 20)
                return $"{obj.Substring(0, 19)}...";
            else return obj;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
