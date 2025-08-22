using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement.Instances;
using System;

namespace Natsurainko.FluentLauncher.Xaml.Converters;

internal partial class InstanceDisplayNameConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not MinecraftInstance instance)
            return null;

        return instance.GetDisplayName();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
