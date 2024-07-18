using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Utils;
using System;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

public class BreadcrumbBarLocalizationConverter : IValueConverter
{
    public string BasePath { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string text)
            return string.Empty;

        return ResourceUtils.GetValue(BasePath, $"_{text}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
