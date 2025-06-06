﻿using Microsoft.UI.Xaml.Data;
using System;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

public partial class InvertBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool b)
            throw new ArgumentException(nameof(value));

        return !b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool b)
            throw new ArgumentException(nameof(value));

        return !b;
    }
}
