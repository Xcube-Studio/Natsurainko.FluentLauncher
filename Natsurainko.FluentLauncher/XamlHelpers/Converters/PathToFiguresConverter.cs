using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

public class PathToFiguresConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var path = value as Path;
        var figures = (path.Data as PathGeometry).Figures;
        return figures;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
