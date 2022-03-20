using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FluentLauncher.Converters
{
    public class AccountTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((string)value == "MicrosoftAccount")
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
