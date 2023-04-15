using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Components.FluentCore;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

public class TeachingTipTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || parameter == null)
            return null;
        if (value is GameCore core)
            return $"{parameter} {core.Id}";
        if (value is ModInfo mod)
            return $"{parameter} {mod.Name}";

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
