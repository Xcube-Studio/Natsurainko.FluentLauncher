using System;
using Windows.UI.Xaml.Data;

namespace FluentLauncher.Converters
{
    public class CoreReleaseTimeSubString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((string)value).Split("T")[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
