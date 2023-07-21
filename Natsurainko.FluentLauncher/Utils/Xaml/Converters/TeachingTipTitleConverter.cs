using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.Classes.Datas.Launch;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

public class TeachingTipTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || parameter == null)
            return null;
        if (value is GameInfo gameInfo)
            return $"{parameter} {gameInfo.AbsoluteId}";
        //if (value is ModInfo mod)
        //    return $"{parameter} {mod.Name}";

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
