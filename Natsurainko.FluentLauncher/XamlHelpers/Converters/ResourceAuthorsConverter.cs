using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.Resources;
using System;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

internal partial class ResourceAuthorsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is CurseForgeResource curseResource)
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
