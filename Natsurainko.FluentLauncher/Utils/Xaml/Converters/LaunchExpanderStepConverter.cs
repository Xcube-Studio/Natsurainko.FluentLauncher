using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.Classes.Enums;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

internal class LaunchExpanderStepConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var needProperty = parameter.ToString();

        return needProperty switch
        {
            "StepName" => ResourceUtils.GetValue("Converters", $"_LaunchState_{value}"),
            "FontForeground" => (int)value switch
            {
                1 or -1 => App.Current.Resources["ApplicationForegroundThemeBrush"],
                _ => App.Current.Resources["ApplicationSecondaryForegroundThemeBrush"]
            },
            "FontWeight" => (int)value switch
            {
                1 or -1 => FontWeights.SemiBold,
                _ => FontWeights.Normal
            },
            "FontIcon" => (int)value switch
            {
                0 => "\uE73C",
                2 => "\uE73E",
                -1 => "\uE711",
                _ => null
            },
            "FontIconVisibility" => (int)value == 1 ? Visibility.Collapsed : Visibility.Visible,
            "ProgressRingActive" => (int)value == 1,
            "ExitFontIcon" => (LaunchState)value == LaunchState.GameCrashed ? "\uE711" : "\uE73E",
            _ => null
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
