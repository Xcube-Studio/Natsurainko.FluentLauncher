using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.GameManagement.Installer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Natsurainko.FluentLauncher.Xaml.Converters;

internal partial class ResourceFilterCategoriesConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is IEnumerable<string> categories)
            return FilterCategories(categories);

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private static IEnumerable<string> FilterCategories(IEnumerable<string> categories)
    {
        string[] filteredTags = [.. Enum.GetNames<ModLoaderType>().Select(x => x.ToLower())];

        foreach (var category in categories)
        {
            if (filteredTags.Contains(category))
                continue;

            yield return category;
        }
    }
}
