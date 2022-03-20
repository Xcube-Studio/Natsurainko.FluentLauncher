using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FluentLauncher.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((bool)value)
            {
                case true:
                    return Visibility.Visible;
                case false:
                    return Visibility.Collapsed;
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            switch ((Visibility)value)
            {
                case Visibility.Visible:
                    return true;
                case Visibility.Collapsed:
                    return false;
            }

            return true;
        }
    }
}
