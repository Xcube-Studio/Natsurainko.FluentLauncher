using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Utils;
using System;
using System.Collections.Generic;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

public class BreadcrumbBarLocalizationConverter : IValueConverter
{
    public string BasePath { get; set; }

    public List<string> IgnoredText { get; set; } = [];

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string text)
            return string.Empty;

        if (IgnoredText.Contains(text))
            return text;

        return ResourceUtils.GetValue(BasePath, $"_{text}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
