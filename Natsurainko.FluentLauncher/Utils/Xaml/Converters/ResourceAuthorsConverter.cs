using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.Classes.Datas.Download;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

internal class ResourceAuthorsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is CurseResource curseResource)
            return string.Join("\r\n", curseResource.Authors);
        else if (value is ModrinthResource modrinthResource)
            return modrinthResource.Author;

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
