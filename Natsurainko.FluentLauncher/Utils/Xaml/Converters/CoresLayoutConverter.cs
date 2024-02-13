using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Services.Settings;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

internal class CoresLayoutConverter : IValueConverter
{
    private readonly SettingsService settingsService = App.GetService<SettingsService>();

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return settingsService.CoresLayoutIndex switch
        {
            1 => new UniformGridLayout()
            {
                MinItemHeight = 150,
                MinItemWidth = 125,
                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                ItemsJustification = UniformGridLayoutItemsJustification.Start
            },
            2 => new UniformGridLayout()
            {
                MinItemWidth = 300,
                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                ItemsJustification = UniformGridLayoutItemsJustification.Start
            },
            _ => new StackLayout()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
