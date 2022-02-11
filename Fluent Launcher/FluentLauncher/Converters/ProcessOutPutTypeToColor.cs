using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace FluentLauncher.Converters
{
    public class ProcessOutPutTypeToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string type = value.ToString();

            switch (type)
            {
                case "ERROR":
                    return new SolidColorBrush(ColorConverter.FromString("#19F44336"));
                case "FATAL":
                    return new SolidColorBrush(ColorConverter.FromString("#3FF44336"));
                case "WARN":
                    return new SolidColorBrush(ColorConverter.FromString("#19FFC107"));
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
