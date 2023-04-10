using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentCore.Extension.Windows.Service;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

public class JavaInfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var file = (string)value;

        if (string.IsNullOrEmpty(file))
            return null;
        var info = JavaHelper.GetJavaRuntimeInfo(file);

        return $"{info.Name} ({info.Architecture}, {info.Version})";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
