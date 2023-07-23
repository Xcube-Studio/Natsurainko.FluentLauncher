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
            "StepName" => (LaunchState)value switch
            {
                LaunchState.Created => "The launch task is created",
                LaunchState.Inspecting => "Checking required items before launching",
                LaunchState.Authenticating => "Verifying and refreshing account token",
                LaunchState.CompletingResources => "Completing the game dependent resources",
                LaunchState.BuildingArguments => "Generating startup parameters",
                LaunchState.LaunchingProcess => "Launching the game process",
                LaunchState.GameRunning => "The game is running",
                LaunchState.GameExited => "The game has exited normally",
                LaunchState.Faulted => "Launch failed",
                LaunchState.Killed => "The game was forced to quit",
                LaunchState.GameCrashed => "The game crashed",
                _ => null,
            },
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
