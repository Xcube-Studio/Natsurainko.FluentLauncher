using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

internal class BackgrondSettingItemVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        int index = (int)value;

        if (index == 0 && "Mica".Equals(parameter))
            return Visibility.Visible;

        if (index == 1 && "Acrylic".Equals(parameter))
            return Visibility.Visible;

        if (index == 2 && "Solid".Equals(parameter))
            return Visibility.Visible;

        if (index == 3 && "Image".Equals(parameter))
            return Visibility.Visible;

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
