using Microsoft.UI.Xaml.Data;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Utils;
using System;

namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

internal partial class ResourceLocalizedCategoriesConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string category)
        {
            if (ApplicationLanguages.PrimaryLanguageOverride != "zh-Hans" && ApplicationLanguages.PrimaryLanguageOverride != "zh-Hant")
                return string.Concat(category[0].ToString().ToUpper(), category.AsSpan(1));

            try
            {
                return LocalizedStrings.GetString($"ResourceCategories__{category
                    .Replace(",", string.Empty)
                    .Replace("\'", string.Empty)
                    .Replace(" ", "_")
                    .Replace("&", string.Empty)
                    .Replace('-', '_')}");
            }
            catch { }

            return category;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
