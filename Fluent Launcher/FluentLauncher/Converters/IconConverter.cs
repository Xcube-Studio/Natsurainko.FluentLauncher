using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentLauncher.Converters
{
    public class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            try
            {
                return new BitmapImage(new Uri($"ms-appx:///Assets/Icons/{value}.png", UriKind.RelativeOrAbsolute));
            }
            catch { return null; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
