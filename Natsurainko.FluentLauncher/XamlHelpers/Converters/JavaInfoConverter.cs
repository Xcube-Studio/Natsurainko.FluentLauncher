using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.Environment;
using System;
using System.IO;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

public class JavaInfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var file = value.ToString();

        if (!File.Exists(file))
            return "Executable file does not exist";

        var info = JavaUtils.GetJavaInfo(file);

        return $"{info.Name} ({info.Architecture}, {info.Version})";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
