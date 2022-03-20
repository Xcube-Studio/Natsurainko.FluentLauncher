using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FluentLauncher.Converters
{
    public class ListInfoBadgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
