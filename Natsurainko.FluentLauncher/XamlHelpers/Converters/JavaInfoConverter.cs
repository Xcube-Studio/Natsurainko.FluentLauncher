using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Environment;
using System;
using System.IO;

namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

public partial class JavaInfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string path ||
            !FileInfoExtensions.TryParse(path, out var fileInfo) || !fileInfo.Exists ||
            (fileInfo.LinkTarget is not null && !File.Exists(fileInfo.LinkTarget)))
            return LocalizedStrings.Converters__InvalidExecutableFile;

        var info = JavaUtils.GetJavaInfo(path);
        return $"{info.Name} ({info.Architecture}, {info.Version})";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
