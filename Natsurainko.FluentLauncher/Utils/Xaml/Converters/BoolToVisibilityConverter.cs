using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

class BoolToVisibilityConverter : IValueConverter
{
    public bool Inverted { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool b)
            throw new ArgumentException("Must be a bool", nameof(value));

        if (Inverted) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
