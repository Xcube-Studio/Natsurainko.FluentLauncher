using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;

namespace FluentLauncher.Converters
{
    public class ApplicationThemeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (ElementTheme)value switch
            {
                ElementTheme.Light => "Light",
                ElementTheme.Dark => "Dark",
                ElementTheme.Default => "Light",
                _ => "Light",
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
