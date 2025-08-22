using Microsoft.UI.Xaml.Data;
using System;
using Windows.UI;

namespace Natsurainko.FluentLauncher.Xaml.Converters;

public partial class ColorHexCodeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Color color)
            return System.Drawing.ColorTranslator.ToHtml(
                System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B));

        return System.Drawing.ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(255, default));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
