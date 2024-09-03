using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using System;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

internal class TaskStateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is TaskState state)
            return ResourceUtils.GetValue("Converters", $"_TaskState_{state}");

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
